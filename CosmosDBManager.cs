using System.Net;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using System.Net.Http.Json;
using System.Text;

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
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.ForegroundColor = ConsoleColor.Yellow;

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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Red;
            Exception baseException = ce.GetBaseException();
            Console.WriteLine("{0} error occurred: {1}", ce.StatusCode, ce);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: {0}", e);
        }
        finally
        {
            Console.ResetColor();
        }
        return false;
    }

    public async Task<bool> CreateDatabase(string dataBaseName, int throughput = 400)
    {
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine("Creating database...\n");

        try
        {
            // Create the throughput properties with the specified throughput value
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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Red;

            Exception baseException = ce.GetBaseException();
            Console.WriteLine("{0} error occurred: {1}", ce.StatusCode, ce);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: {0}", e);
        }
        finally
        {
            Console.ResetColor();
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
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.ForegroundColor = ConsoleColor.Yellow;

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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Red;

            Exception baseException = ce.GetBaseException();
            Console.WriteLine("{0} CosmosException occurred: {1}", ce.StatusCode, ce);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: {0}", e);
        }
        finally
        {
            Console.ResetColor();
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
                                            int? throughput,
                                            IndexingPolicy indexingPolicy = null)
    {
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.ForegroundColor = ConsoleColor.Yellow;

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
            stringBuilder.AppendLine($"IndexingPolicy: {containerPropertiesResponse.IndexingPolicy}");

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
        finally
        {
            Console.ResetColor();
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
    /*

        DeleteContainer(string databaseName, string containerName): This method would delete an existing container from the specified database.

        GetContainer(string databaseName, string containerName): This method would retrieve an existing container from the specified database.

        GetContainers(string databaseName): This method would return a list of all containers in the specified database.
    */
}
