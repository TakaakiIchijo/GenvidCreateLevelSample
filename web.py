#!/usr/bin/env python3

import os
from typing import List

from genvid.toolbox import (ConsulTemplateTool, SDK, Clusters, Profile,
                            CertificateGenerator)


class WebSample(Clusters, ConsulTemplateTool):
    NAME = "Web sample"
    DESCRIPTION = "Web sample script"

    CONFIG_FILES = [
        dict(name="stream", required=True),
        dict(name="events", required=True),
        dict(name="web", required=True),
        dict(name="twitchext", required=False),
        dict(name="sample", required=True),
        dict(name="game", required=True),
    ]
    "The configuration files to load in order. The order is important as some file may override some values"

    def __init__(self):
        super().__init__()
        self.base_dir = os.path.dirname(os.path.abspath(__file__))
        self.images_dir = os.path.join(self.base_dir, "images")
        self._sdk = None
        self.cluster_id = "local"

    @property
    def sdk(self) -> SDK:
        if self._sdk is None:
            self._sdk = SDK(cluster_id=self.cluster_id)
        return self._sdk

    def env(self):
        self.print_env()

    def pyrun(self, *args, **kwargs):
        env = os.environ.copy()
        del env["CURDIR"]
        super().pyrun(*args, env=env)

    def get_domain_name(self):
        output = self.terraform_do_instance_output(instance_id=self.cluster_id)
        domain_name = output.get("domain_name")
        if (self.cluster_id == "local") or (domain_name is None):
            return ""
        else:
            return self.cluster_id + "." + domain_name["value"]

    def get_leaf_endpoint(self):
        output = self.terraform_do_instance_output(instance_id=self.cluster_id)
        leaf_endpoint = output.get("endpoint_leaf")
        if (self.cluster_id == "local") or (leaf_endpoint is None):
            return ""
        else:
            return leaf_endpoint["value"]

    def get_web_endpoint(self):
        output = self.terraform_do_instance_output(instance_id=self.cluster_id)
        web_endpoint = output.get("endpoint_web")
        if (self.cluster_id == "local") or (web_endpoint is None):
            return ""
        else:
            return web_endpoint["value"]

    def build(self):
        self.pyrun(os.path.join(self.base_dir, "build.py"), "all")

    def build_cloud(self):
        self.build()
        self.pyrun(os.path.join(self.base_dir, "build-docker.py"), "all")

    def get_template(self, name):
        folder = "local" if self.cluster_id == "local" else "cloud"
        template_path = os.path.join(self.base_dir, "templates", folder,
                                     name + ".nomad.tmpl")
        with open(template_path) as template_file:
            return template_file.read()

    def get_config(self, target: str, required: bool = True) -> dict:
        for ext in (".hcl", ".json"):
            file_path = os.path.join(self.base_dir, "config", target + ext)
            if os.path.exists(file_path):
                break
        else:
            if not required:
                return {}
            elif target == "stream":
                self.logger.error("stream config file doesn't exist.")
                # trigger an error by opening the file that doesn't exist
                open(file_path)

        env = os.environ.copy()
        env.setdefault("PROJECTDIR", self.base_dir)
        return self.load_config_template(file_path, env=env)

    def merge_config(self, config: dict, targets: List[str]):
        targets = targets if targets else [
            f["name"] for f in self.CONFIG_FILES
        ]

        for config_file in self.CONFIG_FILES:
            name = config_file["name"]
            required = config_file["required"]
            if name in targets:
                partial = self.get_config(name, required)
                self.sdk.merge_dict(partial, config)

        return config

    def load(self, ssl):
        if ssl:
            if self.cluster_id == "local":
                cg = CertificateGenerator(self.sdk)
                cg.generate_ssl(outputdir=self.base_dir)
                os.environ["SSLWEBENDPOINT"] = "localhost"
            else:
                print(
                    "The -s or --ssl option is only to be used with a local cluster. Use an alb_ssl module to use SSL on a cloud cluster."
                )
        config = self.loadEndpoint()
        os.environ["WEBENDPOINT"] = config['config']['cloud']['endpoint'][
            'web']
        config = self.merge_config(config, [])
        # Load the raw template into the configuration
        for key, job in config.setdefault("job", {}).items():
            job["template"] = self.get_template(key)
        self.sdk.set_config(config)
        # Fix to make sure that we can load the endpoint

    def loadEndpoint(self):
        leaf_endpoint = self.get_leaf_endpoint()
        web_endpoint = self.get_web_endpoint()
        config = {
            'version': '1.7.0',
            'config': {
                'cloud': {
                    'endpoint': {
                        'leaf': '',
                        'web': ''
                    }
                }
            }
        }
        config['config']['cloud']['endpoint']['leaf'] = leaf_endpoint
        config['config']['cloud']['endpoint']['web'] = web_endpoint
        return config

    def unload(self):
        config = self.loadEndpoint()
        config = self.merge_config(config, [])
        self.sdk.remove_config(config)

    def upload_images(self,
                      bucket: str = None,
                      path: str = "/images/web",
                      update_config: bool = True,
                      region: str = None):
        prefixes = ["web"]
        self.sdk.upload_images(prefixes,
                               bucket,
                               path,
                               update_config,
                               self.images_dir,
                               region=region)

    COMMANDS = {
        "env": "Print environment variables",
        "build": "Build the specified target",
        "build-cloud": "Build the specified target for the cloud",
        "load": "Load the specified target definition",
        "unload": "Unload the specified target definition in the cloud",
        "upload-images": "Upload tutorial images to the cloud",
    }

    def add_commands(self):
        self.parser.add_argument("-c",
                                 "--cluster_id",
                                 help="The cluster id. Default local")
        for command, help_text in self.COMMANDS.items():
            parser = self.add_command(command, help_text)
            if command == "upload-images":
                parser.add_argument(
                    "-b",
                    "--bucket",
                    default=None,
                    help="Name of the bucket to use.  "
                    "Default is a combination of AWS account, bastion and cluster ids."
                )
                parser.add_argument(
                    "-p",
                    "--path",
                    default="/images/web",
                    help="The path in the bucket (default: %(default)s).")
                parser.add_argument(
                    "-u",
                    "--update-config",
                    action='store_true',
                    help="Upload configuration in the current cluster.")
                parser.add_argument(
                    "-r",
                    "--region",
                    default=None,
                    help=
                    "Region of the S3 bucket where the image will be uploaded.",
                )

            if command == "load":
                parser.add_argument("-s",
                                    "--ssl",
                                    action='store_true',
                                    help="Generate self-signed certificates.")

    def run_command(self, command, options):
        method = command.replace("-", "_")
        self.cluster_id = options.cluster_id or "local"
        del options.cluster_id
        return getattr(self, method)(**vars(options))


def get_parser():
    tool = WebSample()
    return tool.get_parser()


def main():
    profile = Profile()
    profile.apply()

    os.environ.setdefault("GENVID_TOOLBOX_LOGLEVEL", "INFO")

    tool = WebSample()
    return tool.main(profile=profile)


if __name__ == '__main__':

    exit(main() or 0)
