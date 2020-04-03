# Framework for an Industrial Service Bus with IOT Edge

Welcome to our approach to design an independant framework of an industrial service bus which can run on the factory floor. With our initial design session, we had more than 30+ participants from various companies, like #Intel, #HPE, #Senseering and many others, which contributed to designs, code, use cases and prerequsite definitions.
Since this is the first start of our community approach, we are going to continously improve this Github Repo and work on the documentation and features.
The little architecture drawing below shows the overall architecture and some functionality, which we are going to explain in the ["docs"](docs/README.md)  section. The code for a fast implementation can be found in the ["src"](src/README.md) section.

![isb_iot_edge](docs/img/isb_overall_arch.jpg) 

### Content Description:
#### Documentation:

Link to:
* [use cases](docs/USECASES.md) 
* [requriements](docs/REQUIREMENTS.md) 
* [architecture](docs/ARCHITECTURE.md)  

### and most important: 
* [Link to a code example with IOT Edge](src/README.md)

## What this Github Repo is not:
* we do not want to provide an Industrial Service Bus (ISB) as a product.

## What this Github Repo is about:
* provide an OpenSource framework to use any ISB and be independant from its individual feature set
* helping others to easily implement and test a modern architecture to do the next step towards a new generation of a digitized enabled production

## Next Steps:
* Implementing [Nats.io](https://nats.io/) instead of [RabbitMQ] (https://www.rabbitmq.com/)
* Getting more data import modules, not only OPC-UA
* Bringing [Grafana](https://grafana.com/) with [SQl Server Edge](https://azure.microsoft.com/de-de/services/sql-database-edge/) to Azure IOT Edge
* Creating an easy to use Visual Data Analytics module
*  ......





