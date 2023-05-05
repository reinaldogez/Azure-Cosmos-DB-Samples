# Azure Cosmos DB Samples

## Installation

The project includes a Docker Compose file that allows you to run the Azure Cosmos Emulator within a Docker container. However, if your machine has limited memory and CPU resources, it is advisable to install the emulator directly on your operating system to achieve optimal performance. Running the emulator natively on your machine can help minimize resource consumption and provide a smoother experience when working with the emulator.

[**Install and use the Azure Cosmos DB Emulator for local development and testing**](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21)

## Cosmos Concepts

### Partition Key

### Throughput

When creating a container, the specified throughput is the *maximum* number of Request Units (RU) per second that the container can handle. However, when creating a database, it's the opposite, the specified throughput is the *minimum* amount of Request Units (RU) per second that we want to allocate to it. If you don't specify the throughput while creating a database in Azure Cosmos DB, the system will use the default throughput value, which is 400 Request Units per second (RU/s).

### Indexing Policy

An indexing policy specifies the set of indexes that should be created for a container. An index is a way of organizing data that makes it easier and faster to query. By default, Cosmos DB indexes all properties of a container and creates a range index on all string and numeric properties. However, this can lead to higher indexing costs and longer indexing times, especially for large containers with many properties. To improve performance, we need to define a custom indexing policy to specify the properties and types of indexes to create.

## Optimizing queries for a read database in a CQRS project

### Query Massive example

In this context, we will set up containers focused on query performance and cost. Storage and request units (RUs) expended on writing will be disregarded in this scenario because, as the name suggests, the predominant activity in the database will be query execution.

## JSON files with containers config

### cosmosdb-containers-default-settings.json

This JSON creates the containers in a more common way, with default indexes and the 'id' field being used as the partition key.

### cosmosdb-containers-query-massive.json

This JSON creates containers with a focus on high performance, i.e., the indexes and partition keys are optimized for specific query patterns.

## Project Classes

### CosmosBatchManager
The `CosmosBatchManager` class efficiently inserts a large number of items into an Azure Cosmos DB container using batch requests while adhering to the following restrictions and best practices:

1. **Group items by partition key**: Ensures all operations within a batch request target the same logical partition.
2. **Create separate batches per partition key group**: Allows parallel processing and reduces overall insertion time.
3. **Limit operations and size of each batch**: Restricts each batch to a maximum of 100 operations and a total request size not exceeding 2 MB, as per Cosmos DB limits.
4. **Execute batches individually**: Enables better error handling and implements retry logic for failed operations.

### CosmosDBManager

The `CosmosDBManager` class provides functionality for managing Azure Cosmos DB resources, such as databases and containers, and performing operations like checking connections, creating databases and containers, and inserting bulk items.

Main methods:

- `CheckConnection()`: Tests the connection to the Cosmos DB instance.
- `CreateDatabase(string dataBaseName, int throughput = 400)`: Creates a new database with the specified throughput if it does not already exist.
- `CheckDatabaseExists(string databaseName)`: Checks if the specified database exists.
- `CreateContainersList(List<ContainerInfo> listContainersInfo, string databaseName)`: Creates multiple containers within the specified database using the provided list of `ContainerInfo`.
- `CreateContainer(string databaseName, string containerName, string partitionKeyPath, int? throughput = null, IndexingPolicy indexingPolicy = null)`: Creates a container with the specified properties within the specified database.
- `InsertBulkItemsAsync<T>(string databaseName, string containerName, List<T> items)`: Inserts a bulk of items into the specified container in the specified database.

  For each item in the list of items to be inserted, the method creates a new task by calling the `InsertItemWithRetryAsync` method. This method takes care of inserting a single item and handles retries in case of throttling (i.e., when the Cosmos DB service returns a TooManyRequests status code). The method stores these tasks in a list.

  The method then uses `Task.WhenAll` to wait for all the tasks to complete. The results, an array of request charges (or -1 for failed items), are stored in the `results` variable.

  The method iterates through the results and increments the counters for the total request charge, successful inserts, and failed inserts accordingly.

- `GetContainer(string databaseName, string containerName)`: Retrieves the specified container from the specified database.

### CosmosQueryEngine

The `CosmosQueryEngine` class provides methods to query and read data from an Azure Cosmos DB container.

- Constructor `CosmosQueryEngine(CosmosClient cosmosClient)`: Initializes a new instance of the `CosmosQueryEngine` class with the given `CosmosClient`.

- `QueryItemsAsync<T>(string queryString, string databaseName, string containerName, Dictionary<string, object> parameters = null, bool populateIndexMetrics = false)`: Executes a query against the specified container in the specified database and returns the results as a list of `T` objects. It also returns the total request charge.

- `QuerySingleValueAsync<T>(string queryString, string databaseName, string containerName, Dictionary<string, object> parameters = null, bool populateIndexMetrics = false)`: Executes a query against the specified container in the specified database and returns a single result as a `T` object. It also returns the total request charge and an error message, if any.

- `PointReadAsync<T>(string id, string partitionKeyValue, string databaseName, string containerName)`: Performs a point read operation to retrieve a single item with the given `id` and `partitionKeyValue` from the specified container in the specified database. It returns the item as a `T` object, the total request charge, and an error message, if any.
