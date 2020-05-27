# Industrial Service Bus Implementation

> ## Disclaimer
>
> For the purpose of demonstration we use the default username/password for RabbitMQ running as container on IoT Edge.
> Also, the actual broker configuration is part of this repository.
>
> The automatically created environment is a sandbox meaning that __all VM instances are _NOT_ accessible from the Internet__ and only private IP addresses are used.
>
> __You should _never_ use default credentials in production and/or check in your credentials into Git repositories!__  
> Please consider using [Azure KeyVault](https://docs.microsoft.com/en-us/azure/key-vault/basic-concepts) for secret management and using a decent CI/CD pipeline to roll out your changes in a secure way.

## Folder Structure

This section refers to the folder structure under `src/`.

- `src/deployment` contains all deployment scripts
- `src/iotedge` contains the actual implementation and tests as well as deployment manifests
- `src/rabbitmq-federation` contains the Dockerfile based on the official RabbitMQ base image. It enables the [RabbitMQ federation plugin](https://www.rabbitmq.com/federation.html).

## Implementation Details

This demo implementation is based on [Azure IoT Edge](https://docs.microsoft.com/en-us/azure/iot-edge/about-iot-edge) and revolves around the concept of Readers and Writers.  
In fact these roles are represented by IoT Edge modules, the `ISBWriter` and `ISBReader` respectively.  
The _Writer_ is responsible for reading data from a simulated PLC via OPC UA and __writing__ this data into the ISB using [Dapr](https://dapr.io/) [PubSub](https://github.com/dapr/samples/tree/master/4.pub-sub).  
The _Reader_ on the other hand is responsible for __reading__ data from the ISB, again using Dapr PubSub, and forwarding it to IoT Hub.

The following diagram shows the interaction between different IoT Edge modules and Dapr. In the demo setup `ISBReader` and `ISBWriter` modules are deployed to different IoT Edge nodes for the sake of demonstration (see [deployment diagram](deployment/img/deployment.png)).

![ISB Implementation with IoT Edge](img/isb_iotedge1.png)

Dapr currently supports two modes of operation:

- _Standalone_: running locally
- _Kubernetes_: running on Kubernetes cluster

IoT Edge is none of those, but is closer to the _Standalone_ option.  
Both options use the [Sidecar pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/sidecar). [This blog post](https://medium.com/@vslepakov/dapr-on-azure-iot-edge-31c7020c8cda) goes into details on how to run Dapr on IoT Edge.  

Right now, [RabbitMQ](https://www.rabbitmq.com/) is used as the underlying message broker.
Since we use Dapr we can easily switch to another implementation (e.g. [NATS.io](https://nats.io/)) by simply modifying the [Dapr component configuration](iotedge/modules/ISBWriter/components/rabbitmq.yaml) without any changes to the application itself.  
In order to configure Federation of multiple RabbitMQ broker instances (two _Writers_, one _Reader_) the _RabbitMQManager_ module is used. It reads federation configuration from the IoT Edge Module Twin and applies it to the target broker instance at runtime. Unit tests for this module can be found in the `src/iotedge/tests` directory.

There are two deployment __templates__ for _Reader_ and _Writer_ roles respectively:

- `deployment.isbwriter.template.json`
- `deployment.isbreader.template.json`

They define IoT Edge modules for each role.

> Also, the generated deployment __manifests__ under `src/iotedge/config` are part of this repository, even though normally one would not check them in.
This is done on purpose to get you up and running with as little effort as possible.
