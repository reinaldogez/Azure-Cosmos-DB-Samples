using System.Net;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using System.Text;
using System.Diagnostics;

namespace AzureCosmosDBSamples;

public class CosmosDBManager
{
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosDbSettings _cosmosDbSettings;

    public CosmosDBManager(CosmosClient cosmosClient, CosmosDbSettings cosmosDbSettings)
    {
        _cosmosClient = cosmosClient;
        _cosmosDbSettings = cosmosDbSettings;
    }
    public async Task<bool> CheckConnection()
    {
        Console.WriteLine("Testing Connection...\n");

        try
        {
            AccountProperties accountProperties = await _cosmosClient.ReadAccountAsync();
            ListProperties(accountProperties);
            Console.WriteLine("Connection to Cosmos Emulator is Ok.");
            return true;
        }
        catch (CosmosException ce)
        {
            Exception baseException = ce.GetBaseException();
            Console.WriteLine("{0} error occurred: {1}", ce.StatusCode, ce);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
        }
        return false;
    }

    public async Task<bool> CreateDatabase(string dataBaseName, int throughput = 400)
    {
        Console.WriteLine("Creating database...\n");

        try
        {
            ThroughputProperties throughputProperties = ThroughputProperties.CreateManualThroughput(throughput);
            DatabaseResponse databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(dataBaseName, throughputProperties: throughputProperties);
            if (databaseResponse.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine($"The database '{dataBaseName}' already exists.");
                return true;
            }
            else if (databaseResponse.StatusCode == HttpStatusCode.Created)
            {
                Console.WriteLine($"The database '{dataBaseName}' was created.");
                return true;
            }
        }
        catch (CosmosException ce)
        {
            Exception baseException = ce.GetBaseException();
            Console.WriteLine("{0} error occurred: {1}", ce.StatusCode, ce);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
        }
        return false;
    }

    private static void ListProperties(object obj)
    {
        if (obj == null) return;

        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            Console.WriteLine($"{property.Name}: {property.GetValue(obj)}");

            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                ListProperties(property.GetValue(obj));
            }
        }
    }

    public async Task<bool> CheckDatabaseExists(string databaseName)
    {
        Console.WriteLine("Checking database...\n");

        try
        {
            var database = _cosmosClient.GetDatabase(databaseName);
            var databaseResponse = await database.ReadAsync();
            if (databaseResponse.StatusCode == HttpStatusCode.OK)
            {
                int? throughput = await databaseResponse.Database.ReadThroughputAsync();
                DatabaseProperties databaseProperties = databaseResponse.Resource;
                var databaseId = databaseProperties.Id;
                var databaseSelfLink = databaseProperties.SelfLink;
                var databaseEtag = databaseProperties.ETag;

                var sb = new StringBuilder();
                sb.AppendLine($"Database {databaseName} exists");
                sb.AppendLine($"Database ID: {databaseId}");
                sb.AppendLine($"Database Self Link: {databaseSelfLink}");
                sb.AppendLine($"Database ETag: {databaseEtag}");
                sb.AppendLine($"Database Throughput: {throughput}");
                Console.WriteLine(sb.ToString());
                return true;
            }
            else
                Console.WriteLine($"Database {databaseName} status code {databaseResponse.StatusCode}");
            return false;
        }
        catch (CosmosException ce)
        {
            Exception baseException = ce.GetBaseException();
            Console.WriteLine("{0} CosmosException occurred: {1}", ce.StatusCode, ce);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
        }
        return false;
    }

    public async Task<bool> CreateContainersList(List<ContainerInfo> listContainersInfo, string databaseName)
    {
        try
        {
            foreach (var containerInfo in listContainersInfo)
            {
                IndexingPolicy indexingPolicy = new()
                {
                    IndexingMode = ParseIndexingMode(containerInfo.IndexingMode)
                };

                if (containerInfo.IncludedPaths != null)
                {
                    foreach (string includedPath in containerInfo.IncludedPaths)
                    {
                        indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = includedPath });
                    }
                }

                if (containerInfo.ExcludedPaths != null)
                {
                    foreach (string excludedPath in containerInfo.ExcludedPaths)
                    {
                        indexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = excludedPath });
                    }
                }

                await CreateContainer(databaseName, containerInfo.Id, containerInfo.PartitionKeyPath, containerInfo.Throughput, indexingPolicy);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating containers in database '{databaseName}': {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CreateContainer(string databaseName,
                                            string containerName,
                                            string partitionKeyPath,
                                            int? throughput = null,
                                            IndexingPolicy indexingPolicy = null)
    {
        Console.WriteLine("Creating container...\n");

        try
        {
            var database = _cosmosClient.GetDatabase(databaseName);

            var containerProperties = new ContainerProperties(containerName, partitionKeyPath);
            if (indexingPolicy != null)
            {
                containerProperties.IndexingPolicy = indexingPolicy;
            }
            var throughputProperties = ThroughputProperties.CreateManualThroughput(throughput ?? 400);

            var containerResponse = await database.CreateContainerIfNotExistsAsync(
                containerProperties,
                throughputProperties);

            if (containerResponse.StatusCode == HttpStatusCode.Created)
            {
                Console.WriteLine($"Container {containerName} created in database {databaseName}.");
            }
            else if (containerResponse.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine($"Container {containerName} already exists in database {databaseName}.");
            }

            var containerResourceResponse = await containerResponse.Container.ReadContainerAsync();
            var containerPropertiesResponse = containerResourceResponse.Resource;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Container Properties:");
            stringBuilder.AppendLine($"Id: {containerPropertiesResponse.Id}");
            stringBuilder.AppendLine($"SelfLink: {containerPropertiesResponse.SelfLink}");
            stringBuilder.AppendLine($"ETag: {containerPropertiesResponse.ETag}");
            stringBuilder.AppendLine($"PartitionKeyPath: {containerPropertiesResponse.PartitionKeyPath}");
            stringBuilder.AppendLine($"DefaultTimeToLive: {containerPropertiesResponse.DefaultTimeToLive}");
            foreach (IncludedPath includedPath in containerPropertiesResponse.IndexingPolicy.IncludedPaths)
                stringBuilder.AppendLine($"IndexingPolicy IncludedPath: {includedPath.Path}");
            foreach (ExcludedPath excludedPath in containerPropertiesResponse.IndexingPolicy.ExcludedPaths)
                stringBuilder.AppendLine($"IndexingPolicy ExcludedPath: {excludedPath.Path}");

            Console.WriteLine(stringBuilder.ToString());

            return true;
        }
        catch (CosmosException ex)
        {
            Console.WriteLine($"CosmosException: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            return false;
        }
    }

    private IndexingMode ParseIndexingMode(string indexingModeString)
    {
        return indexingModeString.ToLowerInvariant() switch
        {
            "consistent" => IndexingMode.Consistent,
            "lazy" => IndexingMode.Lazy,
            "none" => IndexingMode.None,
            _ => throw new ArgumentException($"Invalid IndexingMode string: {indexingModeString}")
        };
    }

    public async Task InsertBulkItemsAsync<T>(string databaseName, string containerName, List<T> items)
    {
        Console.WriteLine($"Starting bulk insert of {items.Count} items into container {containerName}...");

        int totalRequestCharge = 0;
        int insertedCount = 0;
        int failedCount = 0;

        Container container = GetContainer(databaseName, containerName);

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        List<Task<int>> tasks = new List<Task<int>>(items.Count);

        foreach (var item in items)
        {
            tasks.Add(InsertItemWithRetryAsync(container, item));
        }

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            if (result > 0)
            {
                totalRequestCharge += result;
                insertedCount++;
            }
            else
            {
                failedCount++;
            }
        }

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;

        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        Console.WriteLine("\tTotal time: {0}", elapsedTime);

        Console.WriteLine($"Bulk insert of {items.Count} items into container {containerName} complete.");
        Console.WriteLine($"Total request charge: {totalRequestCharge}");
        Console.WriteLine($"Inserted items: {insertedCount}");
        Console.WriteLine($"Failed items: {failedCount}");
    }

    private async Task<int> InsertItemWithRetryAsync<T>(Container container, T item, int maxRetries = 5)
    {
        int currentRetry = 0;

        while (currentRetry <= maxRetries)
        {
            try
            {
                ItemResponse<T> response = await container.CreateItemAsync(item);
                return (int)response.RequestCharge;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                if (currentRetry == maxRetries)
                {
                    return -1;
                }

                currentRetry++;

                // Wait for the recommended delay before retrying
                TimeSpan retryDelay = ex.RetryAfter ?? TimeSpan.FromSeconds(1);
                await Task.Delay(retryDelay);
            }
        }

        return -1;
    }

    public Container GetContainer(string databaseName, string containerName)
    {
        Console.WriteLine($"Getting container: {containerName}");

        try
        {
            Database database = _cosmosClient.GetDatabase(databaseName);
            Container container = database.GetContainer(containerName);
            return container;
        }
        catch (CosmosException ex)
        {
            Console.WriteLine($"CosmosException: {ex.Message}");
            Console.WriteLine($"StatusCode: {ex.StatusCode}");
            Console.WriteLine($"ActivityId: {ex.ActivityId}");
            Console.WriteLine($"InnerException: {ex.InnerException}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"InnerException: {ex.InnerException}");
            return null;
        }
    }

    /*
        DeleteContainer(string databaseName, string containerName): This method would delete an existing container from the specified database.

        GetContainers(string databaseName): This method would return a list of all containers in the specified database.
    */
}
