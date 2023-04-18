namespace AzureCosmosDBSamples;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

public class CosmosDBManager
{
    private readonly string _primaryKey;
    private readonly string _endpointUri;
    private readonly string _cosmosDatabaseId;


    public CosmosDBManager(IConfiguration configuration)
    {
        _primaryKey = configuration.GetValue<string>("PrimaryKey");
        _endpointUri = configuration.GetValue<string>("EndpointUri");
        _cosmosDatabaseId = configuration.GetValue<string>("CosmosDatabaseId");
    }
    public async Task<bool> CheckConnection()
    {
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine("Testing Connection...\n");

        Console.ResetColor();
        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            try
            {
                await client.CreateDatabaseIfNotExistsAsync(_cosmosDatabaseId);
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
            finally
            {
                client.Dispose();
            }
            return false;
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
