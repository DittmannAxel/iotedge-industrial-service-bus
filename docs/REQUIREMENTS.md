
# Requirements of an ISB / ISB Architecture

## High Availability of Production Data on the Factory Floor

- Data must be disaster resilient, e.g. the same data must be physically stored in different fire compartments
- Data failover in case of an outage of one of the nodes
- Self Healing (container based mechanism)

## Data Governance

- Export restricted data should not leave the country borders (e.g. Defense sector)
- Massive amount of data with only local significance, e.g. local dashboards or indicators
- Data access restrictions

## Runtime

- The solution should be container-based
- Asynchronous communication, "fire-and-forget"

## Disconnect

- No service interruption if Internet connection is gone

## Data Structure Independency

- Not affected by firmware upgrades of machine triggering data structure changes, e.g. changes to SAP numbers (24 digits), where only 8 digits are used

## And Many More

- High performance (in throughput and replication)
- Resilient, stable / fault tolerant, support for multi-node replication
- Scalable over multiple instances (clustering and federation)
- Support for running on Windows and Linux
- Running in plant and cloud environments—should run local only, if required
- Persistent messages with replay function (streaming)
- Support for Pub/Sub
- Can handle hundreds of microservices (scalability and extensability)
- Data redundancy (replication of data to avoid data loss and provide high availability)
- Flexible routing support
- Support for multiple protocols (at least MQTT, HTTP)
- SDKs for different languages
- Management UI and API
- Tracing, logging
- Commercial support
- Quality of Service (QoS): Support for at-least-once delivery
- Security (support for tokens, certificates, users and passswords, encryption)
