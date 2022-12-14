version = "1.7.0"

secrets {
  disco {
    GENVID_DISCO_SECRET = "discosecret"
  }
  studio {
    GENVID_STUDIO_SECRET = "studiosecret"
  }
  webgateway {
    GENVID_WEBGATEWAY_SECRET = "webgatewaysecret"
  }
}

settings {
  encode {
    stream{
      enable  = true
      service = "twitch"
      addr    = "rtmp://live.twitch.tv/app"
      channel = "drkk_hq"
      key     = "live_475047977_qs0Bvy7y8DXEXI3D8wEISMFZ2OW2ET"
    }
    input {
      width  = 1280
      height = 720
    }
    output {
      width  = 1280
      height = 720
      framerate = 30
    }
  }
  info {
    description = "Sample to demonstrate genvid"
    game        = "Cube"
    name        = "Cube Sample"
  }
}
config {
  local {
    appdir = "{{env `PROJECTDIR` | js}}\\app"
  }
}