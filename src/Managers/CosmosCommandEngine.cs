using System.Net;
using Microsoft.Azure.Cosmos;

namespace AzureCosmosDBSamples.Managers;

public class CosmosCommandEngine
{
    private readonly CosmosClient _cosmosClient;

    public CosmosCommandEngine(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task<(T Item, double RequestCharge, string ErrorMessage)> InsertItemAsync<T>(
        T item,
        string databaseName,
        string containerName,
        string partitionKeyValue)
    {
        try
        {
            Container container = _cosmosClient.GetContainer(databaseName, containerName);
            ItemResponse<T> response = await container.CreateItemAsync<T>(item, new PartitionKey(partitionKeyValue));
            double totalRequestCharge = response.RequestCharge;
            return (response.Resource, totalRequestCharge, string.Empty);
        }
        catch (CosmosException ce)
        {
            Exception baseException = ce.GetBaseException();
            return (default(T), 0, $"Exception: Status code {ce.StatusCode}. Container name: {containerName}.\nException details: {ce}");
        }
        catch (Exception e)
        {
            return (default(T), 0, $"Exception: {e.Message}");
        }
    }

    public async Task<(T Item, double RequestCharge, string ErrorMessage)> UpdateItemAsync<T>(
        T item,
        string databaseName,
        string containerName,
        string partitionKeyValue)
    {
        try
        {
            Container container = _cosmosClient.GetContainer(databaseName, containerName);
            string itemId = typeof(T).GetProperty("id").GetValue(item).ToString();
            ItemResponse<T> response = await container.ReplaceItemAsync<T>(item, itemId, new PartitionKey(partitionKeyValue));
            double totalRequestCharge = response.RequestCharge;
            return (response.Resource, totalRequestCharge, string.Empty);
        }
        catch (CosmosException ce)
        {
            Exception baseException = ce.GetBaseException();
            return (default(T), 0, $"Exception: Status code {ce.StatusCode}. Container name: {containerName}.\nException details: {ce}");
        }
        catch (Exception e)
        {
            return (default(T), 0, $"Exception: {e.Message}");
        }
    }

    public async Task<(bool Success, double RequestCharge, string ErrorMessage)> DeleteItemAsync<T>(
        string id,
        string partitionKeyValue,
        string databaseName,
        string containerName)
    {
        try
        {
            Container container = _cosmosClient.GetContainer(databaseName, containerName);
            ItemResponse<T> response = await container.DeleteItemAsync<T>(id, new PartitionKey(partitionKeyValue));
            double requestCharge = response.RequestCharge;

            return (true, requestCharge, string.Empty);
        }
        catch (CosmosException ce)
        {
            if (ce.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Item not found. Item ID: {id}. Exception: {ce.Message}");
            }
            Exception baseException = ce.GetBaseException();
            return (false, 0, $"Exception: Status code {ce.StatusCode}. Item ID: {id}.\nException details: {ce}");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
            return (false, 0, $"Exception: {e.Message}");
        }
    }

}