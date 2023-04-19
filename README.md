# Azure Cosmos DB Samples

## Installation

The project includes a Docker Compose file that allows you to run the Azure Cosmos Emulator within a Docker container. However, if your machine has limited memory and CPU resources, it is advisable to install the emulator directly on your operating system to achieve optimal performance. Running the emulator natively on your machine can help minimize resource consumption and provide a smoother experience when working with the emulator.

[Install and use the Azure Cosmos DB Emulator for local development and testing](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21)

## Cosmos Concepts

### Throughput
When creating a container, the specified throughput is the maximum number of Request Units (RU) per second that the container can handle.
However, when creating a database, it's the opposite, the specified throughput is the minimum amount of Request Units (RU) per second that we want to allocate to it. If you don't specify the throughput while creating a database in Azure Cosmos DB, the system will use the default throughput value, which is 400 Request Units per second (RU/s).

### Indexing Policy 
An indexing policy specifies the set of indexes that should be created for a container. An index is a way of organizing data that makes it easier and faster to query.
By default, Cosmos DB indexes all properties of a container, and creates a range index on all string and numeric properties. However, this can lead to higher indexing costs and longer indexing times, especially for large containers with many properties.
To improve performance, we need to define a custom indexing policy to specify the properties and types of indexes to create.