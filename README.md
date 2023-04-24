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

## Optimizing queries for a read database in a CQRS project

### Query Massive example
In this context, we will set up containers focused on query performance and cost. Storage and request units (RUs) expended on writing will be disregarded in this scenario because, as the name suggests, the predominant activity in the database will be query execution.

# Json container creation config

## cosmosdb-containers-default-settings.json
This JSON will create the containers in a more common way, with default indexes and the 'id' field being used as the partition key.

## Project Classes

### CosmosBatchManager
Restrictions on the batch request
- Group items by their partition key.
- Create separate batches for each partition key group.
- Limit the number of operations in each batch to a number below 100 and ensure the total request size does not exceed 2 MB.
- Execute each batch separately, and if needed, implement retries or error handling for failed operations.