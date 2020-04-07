#!/bin/bash

getRandomString() {
  sed "s/[^a-zA-Z0-9]//g" <<< $(openssl rand -base64 4) | tr '[:upper:]' '[:lower:]'
}

deployIoTEdge() {
  # Create an IoT Edge virtual machine and configure IoT Edge
  az vm create --resource-group ${RG_NAME} --name $1 --image UbuntuLTS \
    --vnet-name ${VNET_NAME} --subnet ${SUBNET_NAME} --nsg ${NSG_NAME} --public-ip-address "" \
    --generate-ssh-keys --size ${VM_SIZE} --admin-username ${ADMIN_USERNAME} --custom-data ${CLOUD_INIT_IOT_EDGE}

  # Create IoT Edge identity in IoT Hub
  az iot hub device-identity create -n ${ISB_IOT_HUB} -d $1 --ee

  # Deploy edge modules
  az iot edge deployment create -d $1 -n ${ISB_IOT_HUB} \
    --content $2 --target-condition "deviceId='$1'" --priority 10

  local connectionString=$(az iot hub device-identity show-connection-string --device-id $1 --hub-name ${ISB_IOT_HUB} -o tsv)
  az vm run-command invoke -g ${RG_NAME} -n $1 \
    --command-id RunShellScript --script "/etc/iotedge/configedge.sh '${connectionString}'"
}

deployPlc() {
  # Create a PLC virtual machine
  az vm create --resource-group ${RG_NAME} --name $1 --image UbuntuLTS \
    --vnet-name ${VNET_NAME} --subnet ${SUBNET_NAME} --nsg ${NSG_NAME} --public-ip-address "" \
    --generate-ssh-keys --size ${VM_SIZE} --custom-data ${CLOUD_INIT_PLC} --admin-username ${ADMIN_USERNAME} --no-wait
}

RG_NAME=isb-federation-demo
VNET_NAME=isb-demo-vnet
SUBNET_NAME=isb-demo-subnet
NSG_NAME=isb-demo-nsg
BASTION_PUBLIC_IP=isb-bastion-piblic-ip-$(getRandomString)
VM_SIZE=Standard_B1ms
IOT_EDGE_VM_NAME_PREFIX=isb-demo-iotedge
PLC_VM_NAME_PREFIX=isb-demo-plc
ISB_IOT_HUB=isb-demo-iot-hub-$(getRandomString)
BASTION_NAME=isb-azure-bastion-$(getRandomString)
CLOUD_INIT_PLC=cloud-init-plc.yml
CLOUD_INIT_IOT_EDGE=cloud-init-iotedge.yml
ADMIN_USERNAME=azureuser
IOT_EDGE_READER_DEPLOYMENT="../iotedge/config/deployment.isbreader.amd64.json"
IOT_EDGE_WRITER_DEPLOYMENT="../iotedge/config/deployment.isbwriter.amd64.json"

# Install extensions
az extension add --name azure-iot
az extension add --name azure-cli-iot-ext

# Remove old .env file
rm .env

set -e
trap 'catch $?' EXIT
catch() {
  # echo "catching!"
  if [ "$1" != "0" ]; then
    # error handling goes here
    echo "Error $1 occurred"
  fi
}

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

# Create a resource group.
az group create --name ${RG_NAME}

# Create IoT Hub
az iot hub create --resource-group ${RG_NAME} --name ${ISB_IOT_HUB} --sku S1

# Create a virtual network and front-end subnet.
az network vnet create --resource-group ${RG_NAME} --name ${VNET_NAME} --address-prefix 10.0.0.0/16 \
  --subnet-name ${SUBNET_NAME} --subnet-prefix 10.0.0.0/24

# Create AzureBastionSubnet
az network vnet subnet create --resource-group ${RG_NAME} --vnet-name ${VNET_NAME} \
  --name AzureBastionSubnet --address-prefix 10.0.1.0/27

# Create public IP for Azure Bastion
az network public-ip create --resource-group ${RG_NAME} --name ${BASTION_PUBLIC_IP} \
  --allocation-method Static --sku Standard

# Create Azure Bastion
az network bastion create --name ${BASTION_NAME} --public-ip-address ${BASTION_PUBLIC_IP} \
  --resource-group ${RG_NAME} --vnet-name ${VNET_NAME}

# Prepare PLCs
deployPlc ${PLC_VM_NAME_PREFIX}-1
deployPlc ${PLC_VM_NAME_PREFIX}-2

# Prepare IoT Edge Devices
deployIoTEdge ${IOT_EDGE_VM_NAME_PREFIX}-1 ${IOT_EDGE_WRITER_DEPLOYMENT}
deployIoTEdge ${IOT_EDGE_VM_NAME_PREFIX}-2 ${IOT_EDGE_WRITER_DEPLOYMENT}
deployIoTEdge ${IOT_EDGE_VM_NAME_PREFIX}-3 ${IOT_EDGE_READER_DEPLOYMENT}

# Write .env file
echo "ISB_IOT_HUB=${ISB_IOT_HUB}" >> .env
echo "IOT_EDGE_1=${IOT_EDGE_VM_NAME_PREFIX}-1" >> .env
echo "IOT_EDGE_2=${IOT_EDGE_VM_NAME_PREFIX}-2" >> .env
echo "IOT_EDGE_3=${IOT_EDGE_VM_NAME_PREFIX}-3" >> .env

# Restart Dapr modules as a workaround for Dapr not yet implementing decent retry mechanism for reconenctions to a pub/sub broker
./restart-dapr.sh

# Output credentials for VMs
echo "YOUR USERNAME FOR USING SSH THROUGH AZURE BASTION:"
echo ${ADMIN_USERNAME}

echo "YOUR SSH PRIVATE KEY FOR USING SSH THROUGH AZURE BASTION:"
cat ~/.ssh/id_rsa
