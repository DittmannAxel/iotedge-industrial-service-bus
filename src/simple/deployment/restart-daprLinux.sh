#!/bin/bash

set -e
trap 'catch $?' EXIT
catch() {
  # echo "catching!"
  if [ "$1" != "0" ]; then
    # error handling goes here
    echo "Error $1 occurred"
  fi
}

read_var() {
  grep $1 .env | cut -d '=' -f2
}

ISB_IOT_HUB=$(read_var ISB_IOT_HUB)
IOT_EDGE_1=$(read_var IOT_EDGE_1)
IOT_EDGE_2=$(read_var IOT_EDGE_2)
IOT_EDGE_3=$(read_var IOT_EDGE_3)
MAX_RETRY_COUNT=10

isDeploymentSuccessful() {
  local output=""
  local retries=0
  while [ "$output" != \"$1\" ] && [ $retries -lt ${MAX_RETRY_COUNT} ]; do
    output=$(az iot edge deployment show-metric -m reportedSuccessfulCount -d $1 --metric-type system  -n ${ISB_IOT_HUB}  | jq '.result[0]')
    sleep 3
    ((retries++))
  done

  [[ $output = \"$1\" ]] && echo 0 || echo 1
}

restartDapr() {
  local status=$(isDeploymentSuccessful $1)
  if [[ $status ]]; then
    echo "Deployment for $1 was successful. Restarting Dapr ..."
    az iot hub invoke-module-method --method-name 'RestartModule' -n ${ISB_IOT_HUB} -d $1 -m '$edgeAgent' --method-payload \
    '
      {
        "schemaVersion": "1.0",
        "id": "isbwriter"
      }
    '
    az iot hub invoke-module-method --method-name 'RestartModule' -n ${ISB_IOT_HUB} -d $1 -m '$edgeAgent' --method-payload \
    '
      {
        "schemaVersion": "1.0",
        "id": "isbreader"
      }
    '
    echo "Dapr restarted."
  else
    echo "Deployment for ${IOT_EDGE_VM_NAME_PREFIX}-${i} was NOT successful."
  fi
}

restartDapr ${IOT_EDGE_1}
restartDapr ${IOT_EDGE_2}
restartDapr ${IOT_EDGE_3}
