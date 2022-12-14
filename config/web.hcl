version = "1.7.0"

job "web" {}

log "web" {
  job      = "web"
  task     = "web"
  fileName = "stdout"
}

log "weberr" {
  job      = "web"
  task     = "web"
  fileName = "stderr"
}

link "web" {
  name     = "Cube Sample"
  template = "http://${serviceEx `web` `` true}/"
}
link "admin" {
  name     = "Cube Admin"
  template = "http://${serviceEx `web` `` true}/admin"
}

// {{with $url := env `SSLWEBENDPOINT`}}
link "web" {
  name     = "Cube Sample"
  template = "https://{{$url}}:30000"
}
link "admin" {
  template = "https://{{$url}}:30000/admin"
}
// {{end}}

// Check if we are using a load balancer
// {{with $url := env `WEBENDPOINT`}}
link "web" {
  name     = "Cube Sample"
  template = "https://{{$url}}/"
}
link "admin" {
  name     = "Cube Admin"
  template = "https://{{$url}}/admin"
}
// {{end}}



config {
  local {
	website {
	  root   = "{{env `PROJECTDIR` | js}}"
	  script = "{{env `PROJECTDIR` | js}}\\backend\\www"
	}
	binary {
	  node {
		path = "{{plugin `where.exe` `node` | js}}"
	  }
	}
  }
  embed_ssl {
    enabled = false
  }
} // end of config
