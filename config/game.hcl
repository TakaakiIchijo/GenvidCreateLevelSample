version = "1.7.0"

job "unity" {
}

log "unity" {
  job      = "unity"
  fileName = "unity.log"
}

log "unityerr" {
  job      = "unity"
  fileName = "stderr"
  logLevel = true
}

config {
  local {
      unity {
        appdir = "{{env `PROJECTDIR` | js}}\\app"
    }
  }
}
