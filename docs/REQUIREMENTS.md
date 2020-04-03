
# Requirements of an ISB / ISB Architecture

### HA of production data on the factory floor
* Data must be disaster resilient: e.g.: same data must be stored in different fire compartments
* Data failover in case of an outage of one of the nodes
* Self healing (container based mechanism)
### Data Governance
* Export restricted data SHOULD not leave the country borders (eg. Defense)
* Massive amount of data with local significance only e.g.: local dashboards, local indicators (OEE of a machnine)
* Data access restriction
### Runtime
* The solution should be a container based solution
* Asynchronous communication, Fire & Forget
### Disconnected
No service interruption, if Internet connection is gone
### Data Structure Independency
After machine Firmware upgrade -> Data structure changed -> no ISB affection
E.g.: SAP numbers (24 digits), only 8 digits are used, etc ..

### and many more:


* High Performance [Throughput & Replication]
* Resilient, Stable / Fault Tolerant, Multi Node Replication
* Scalable over multiple instances (clustering and federation)
* Running on Windows / Linux
* Running in plant and cloud environment, should run local only, if needed
* Persisting of messages with replay function (streaming)
* Pub/Sub
* Can handle hundreds of microservices (MS) -> scalability and extensability
* Data redundandcy [Replication of Data to avoid data loss and provide high availability]
* Flexible Routing 
* Multi Protocol, at least MQTT, HTTP
* SDKs for different languages
* Management UI / API
* Tracing, logging
* Commercial support
* QoS: at least once
* Security requirements (Token, Certificate, User/Pass, Encryption)