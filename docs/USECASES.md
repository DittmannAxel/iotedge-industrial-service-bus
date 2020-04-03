# Usecases
## why to use an ISB with an abstraction layer
The intent is to give an idea why we want to use an ISB architecture on the factory floor and why we want to be independant from a specific product. We collected during several meetings a few use cases to describe some scenarios, which are important in a production environment (of course not a complete list - just to give an idea why we are doing this):

### Uninterrupted production
Uplink to the Internet should not affect productive systems and information should flow at least to local connected machines & services. Required production data to guarantee quality & service delivering from the production floor should be availiable in a redundant way at all times.

### MES Connection to production data
Connection of the installed MES systems to an ISB - Interface for data exchange should be availiable at all times, but also it is important that the data exchange is independant of e.g.: change of the generated data format from the PLC. If we normalize as early as possible (at the IOT Edge level) we do not need to adjust the MES system. The data should be availiable via pull & push.

### Change  in production environment (planned & unplanned)
Several situations can happen:
* Machine moves into different factory areas
* Machine software update
* Machine break down (backup & restore)
* Scaling <br>
  
Again, a bidirectional data flow (from and to) the ISB system is needed.

### Secure Lifecycle – Adapter (Machine)
authentication, authorization and accounting should be implemented in a module e.g.: which connects to the ISB.
Secure Device Onboarding (SDO) should be given in this process

### Secure Lifecycle ISB / Edge Solution
The ISB and all of the IOT Edge components should be updateable and secured In this content, we need especially to update the firmware / containers of the ISB architecture. this should be done centrally and as automated as possible.

### Local data exchange
Data into, from and within should be delivered in a reliable way, since every data point could be crucial in terms of securing the quality of the production. And of course the data should be availiable also local. One reason: speed of data connection and of course the data availiability in case of internet connection loss.

### Seperation of responsibilities  
In many companies there are several departments which have to deal with Information Technology. In this case we can have clear responsibilities for e.g: shop floor IT / central IT / OT departments. the following example may be a better way to explain it: <br>
Central it services provides: Azure IOT Edge, module store, DevOps environment <br>
Shop floor IT configures and maintains the solution inclusive: data governance topics.<br>
The idea is to have local responsibilities in delivering a local service. another way to look at it might be: a seperation between: infrastructure operations / application operations

### Serviceability 
Opensource challenge (is product there for more than 10 years?) – Will there be a commercial support availiable? If the solution is exactly what we need, does the solution provider have a specific size? Has the supporter community a critical mass?<br>
and of course the TCO -> who will maintain the solution?













