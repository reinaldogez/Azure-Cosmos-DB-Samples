namespace AzureCosmosDBSamples;

using System.Net;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

public class CosmosDBManager
{
    private readonly string _primaryKey;
    private readonly string _endpointUri;
    private readonly string _cosmosDatabaseId;
    private readonly string _mongoDBConnectionString;


    public CosmosDBManager(IConfiguration configuration)
    {
        _primaryKey = configuration.GetValue<string>("PrimaryKey");
        _endpointUri = configuration.GetValue<string>("EndpointUri");
        _cosmosDatabaseId = configuration.GetValue<string>("CosmosDatabaseId");
        _mongoDBConnectionString = configuration.GetValue<string>("MongoDBConnectionString");

    }
    public async Task<bool> CheckConnection()
    {
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine("Testing Connection...\n");

        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            try
            {
                AccountProperties accountProperties = await client.ReadAccountAsync();
                // await client.CreateDatabaseIfNotExistsAsync(_cosmosDatabaseId);
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
                client.Dispose();
            }
            return false;
        }
    }

    public async Task<bool> CreateDatabase()
    {
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine("Creating database...\n");

        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            try
            {
                DatabaseResponse databaseResponse = await client.CreateDatabaseIfNotExistsAsync(_cosmosDatabaseId);
                if (databaseResponse.StatusCode == HttpStatusCode.Accepted)
                    Console.WriteLine("Database created");
                else
                    Console.WriteLine($"DatabaseResponse status code {databaseResponse.StatusCode}");
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
                client.Dispose();
            }
            return false;
        }
    }

    public static void ListProperties(object obj)
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
    /*
        CheckDatabaseExists(string databaseName): This method would take a database name as input and check if the database already exists in Cosmos DB. It would return a Boolean value indicating whether the database exists or not.

        CreateDatabase(string databaseName): This method would create a new database with the given name in Cosmos DB.

        CreateContainer(string databaseName, string containerName, string partitionKeyPath, int throughput): This method would create a new container in the specified database with the given name, partition key path, and throughput.

        DeleteContainer(string databaseName, string containerName): This method would delete an existing container from the specified database.

        GetContainer(string databaseName, string containerName): This method would retrieve an existing container from the specified database.

        GetContainers(string databaseName): This method would return a list of all containers in the specified database.
    */
}
