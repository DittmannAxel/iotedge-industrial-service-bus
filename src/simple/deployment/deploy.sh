#!/bin/bash

###################################################################
#Script Name	: ISB Installer                                                                                      
#Description	: this script installs the demo infrastructure on Azure with a running ISBWriter and ISBReader example
#Args           :                                                                                           
###################################################################
#Coloring
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
NC='\033[0m'
echo -e "${GREEN}"
echo -e ' _____  _____ ____  '
echo -e '|_   _|/ ____|  _ \ '
echo -e '  | | | (___ | |_) |'
echo -e '  | |  \___ \|  _ < '
echo -e ' _| |_ ____) | |_) |'
echo -e '|_____|_____/|____/ '
echo -e '                    '
echo -e '                    '
echo -e ' _____ _   _  _____ _______       _      _      ______ _____  '
echo -e '|_   _| \ | |/ ____|__   __|/\   | |    | |    |  ____|  __ \ '
echo -e '  | | |  \| | (___    | |  /  \  | |    | |    | |__  | |__) |'
echo -e '  | | | . ` |\___ \   | | / /\ \ | |    | |    |  __| |  _  / '
echo -e ' _| |_| |\  |____) |  | |/ ____ \| |____| |____| |____| | \ \ '
echo -e '|_____|_| \_|_____/   |_/_/    \_\______|______|______|_|  \_\'

# Errorhandler

set -e
trap 'catch $? $LINENO' EXIT
catch() {
    echo "catching!"
      if [ "$1" != "0" ]; then
    # error handling goes here
    echo "Error $1 occurred on $2"
  fi
}
# Functions
getRandomString() {
  sed "s/[^a-zA-Z0-9]//g" <<< $(openssl rand -base64 4) | tr '[:upper:]' '[:lower:]'
}
##  EDGE RUNTIME
deployIoTEdge() {
  # Create an IoT Edge virtual machine and configure IoT Edge
  echo "Create VM with custom data"
  az vm create --resource-group ${RG_NAME} --name $1 --image UbuntuLTS \
    --vnet-name ${VNET_NAME} --subnet ${SUBNET_NAME} --nsg ${NSG_NAME} --public-ip-address ${PUBLIC_IP} --private-ip-address "10.0.0.4" \
    --generate-ssh-keys --size ${VM_SIZE} --admin-username ${ADMIN_USERNAME} --custom-data ${CLOUD_INIT_IOT_EDGE}

  # Create IoT Edge identity in IoT Hub
  echo "Create IoT identity in IoT Hub"
  az iot hub device-identity create -n ${ISB_IOT_HUB} -d $1 --ee

  # Deploy edge modules
  echo "Edge Modules to VM"
  az iot edge deployment create -d $1 -n ${ISB_IOT_HUB} \
    --content $2 --target-condition "deviceId='$1'" --priority 10

  # set IoT Hub connection string
  echo "set IoT Hub connection string"
  local connectionString=$(az iot hub device-identity show-connection-string --device-id $1 --hub-name ${ISB_IOT_HUB} -o tsv)
  az vm run-command invoke -g ${RG_NAME} -n $1 \
    --command-id RunShellScript --script "/etc/iotedge/configedge.sh '${connectionString}'"

  # start up nats cluster
  echo "Start Nats Cluster"
  az vm run-command invoke -g ${RG_NAME} -n $1 \
    --command-id RunShellScript --script "/usr/share/nats/startup.sh"
}

# VARS
RG_NAME=rg-iotedge-industrial-service-bus-dev
VNET_NAME=isb-demo-vnet
SUBNET_NAME=isb-demo-subnet
NSG_NAME=isb-demo-nsg
PUBLIC_IP=isb-public-ip
VM_SIZE=Standard_B2s
IOT_EDGE_VM_NAME_PREFIX=isb-demo
ISB_IOT_HUB=isb-demo-iot-hub-$(getRandomString)
CLOUD_INIT_IOT_EDGE=cloud-init-iotedge.yml
ADMIN_USERNAME=azureuser
IOT_EDGE_DEPLOYMENT="../iotedge/config/deployment.amd64.json"



# Install extensions
az extension add --name azure-iot
az extension add --name azure-cli-iot-ext


rm -f .env

# Login and optinaly set subscription
az login
if [ -z "$1" ]; then
  echo "No subscription provided. Using default subscription"
else
  az account set --subscription $1
fi

# Configure defaults
if [ -z "$2" ]; then
  echo "No region provided. Using default location [westus]"
  az configure --defaults location=westus
else
  az configure --defaults location=$2
fi

echo -e "${GREEN}\n\nCreate Resource Group... ${NC}"
echo -e "${YELLOW}"
# Create a resource group.
az group create --name ${RG_NAME}
echo -e "${NC}"

echo -e "${GREEN}\n\nCreate IoT Hub... ${NC}"
echo -e "${YELLOW}"
# Create IoT Hub
az iot hub create --resource-group ${RG_NAME} --name ${ISB_IOT_HUB} --sku S1
echo -e "${NC}"

echo -e "${GREEN}\n\nCreate VNET... ${NC}"
echo -e "${YELLOW}"
# Create a virtual network and front-end subnet.
az network vnet create --resource-group ${RG_NAME} --name ${VNET_NAME} --address-prefix 10.0.0.0/16 \
  --subnet-name ${SUBNET_NAME} --subnet-prefix 10.0.0.0/24
echo -e "${NC}"

echo -e "${GREEN}\n\nCreate Public IP... ${NC}"
echo -e "${YELLOW}"
# Create public IP
az network public-ip create --resource-group ${RG_NAME} --name ${PUBLIC_IP} \
  --allocation-method Static --sku Standard
echo -e "${NC}"

# Prepare IoT Edge Devices

deployIoTEdge ${IOT_EDGE_VM_NAME_PREFIX}-1 ${IOT_EDGE_DEPLOYMENT}

echo -e "${GREEN}\n\Open Port 3000 for Grafana... ${NC}"
echo -e "${YELLOW}"
# Create public IP
az vm open-port --resource-group ${RG_NAME} --name ${IOT_EDGE_VM_NAME_PREFIX}-1 --port 3000
echo -e "${NC}"

# Write .env file
echo "ISB_IOT_HUB=${ISB_IOT_HUB}" >> .env
echo "IOT_EDGE_1=${IOT_EDGE_VM_NAME_PREFIX}-1" >> .env

# Restart Dapr modules as a workaround for Dapr not yet implementing decent retry mechanism for reconenctions to a pub/sub broker
#  ./restart-dapr.sh

# Output credentials for VMs
echo "YOUR USERNAME FOR USING SSH:"
echo ${ADMIN_USERNAME}

echo "YOUR SSH PRIVATE KEY FOR USING SSH:"
cat ~/.ssh/id_rsa
