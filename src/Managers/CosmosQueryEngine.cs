using System.Net;
using Microsoft.Azure.Cosmos;

namespace AzureCosmosDBSamples.Managers;

public class CosmosQueryEngine
{
    private readonly CosmosClient _cosmosClient;

    public CosmosQueryEngine(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task<(List<T> Results, double RequestCharge)> QueryItemsAsync<T>(
        string queryString,
        string databaseName,
        string containerName,
        Dictionary<string, object> parameters = null,
        bool populateIndexMetrics = false)
    {
        var container = _cosmosClient.GetContainer(databaseName, containerName);
        QueryDefinition queryDefinition = new QueryDefinition(queryString);

        // Add the parameters to the query definition
        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                queryDefinition.WithParameter(parameter.Key, parameter.Value);
            }
        }

        var query = container.GetItemQueryIterator<T>(
            queryDefinition, requestOptions: new QueryRequestOptions
            {
                PopulateIndexMetrics = populateIndexMetrics,
                MaxItemCount = -1,
            });

        var results = new List<T>();

        double totalRequestCharge = 0;

        string indexMetrics = null;

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();

            results.AddRange(response.ToList());
            totalRequestCharge += response.RequestCharge;

            if (populateIndexMetrics && indexMetrics == null)
                indexMetrics = response.IndexMetrics;
        }

        if (populateIndexMetrics)
            Console.WriteLine("Index Metrics: " + indexMetrics);

        return (results, totalRequestCharge);
    }

    public async Task<(T Value, double RequestCharge, string ErrorMessage)> QuerySingleValueAsync<T>(
        string queryString,
        string databaseName,
        string containerName,
        Dictionary<string, object> parameters = null,
        bool populateIndexMetrics = false)
    {
        try
        {
            Container container = _cosmosClient.GetContainer(databaseName, containerName);

            QueryDefinition queryDefinition = new QueryDefinition(queryString);

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    queryDefinition.WithParameter(parameter.Key, parameter.Value);
                }
            }

            var query = container.GetItemQueryIterator<T>(
                queryDefinition, requestOptions: new QueryRequestOptions
                {
                    PopulateIndexMetrics = populateIndexMetrics,
                    MaxItemCount = 1,
                });

            T result = default(T);
            double totalRequestCharge = 0;

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                if (populateIndexMetrics)
                    Console.WriteLine("Index Metrics: " + response.IndexMetrics);

                if (response.Count > 0)
                {
                    result = response.First();
                }

                totalRequestCharge += response.RequestCharge;
            }
            return (result, totalRequestCharge, string.Empty);
        }
        catch (CosmosException ce)
        {
            if (ce.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Container not found. Container name: {containerName}. Exception: {ce.Message}");
            }
            Exception baseException = ce.GetBaseException();
            return (default(T), 0, $"Exception: Status code {ce.StatusCode}. Container name: {containerName}.\nException details: {ce}");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
            return (default(T), 0, $"Exception: {e.Message}");
        }
    }

    public async Task<(T Value, double RequestCharge, string ErrorMessage)> PointReadAsync<T>(
        string id, string partitionKeyValue, string databaseName, string containerName)
    {
        try
        {
            Container container = _cosmosClient.GetContainer(databaseName, containerName);

            var response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKeyValue));
            double totalRequestCharge = response.RequestCharge;

            return (response.Resource, totalRequestCharge, string.Empty);
        }
        catch (CosmosException ce)
        {
            if (ce.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Item not found. Item ID: {id}. Exception: {ce.Message}");
            }
            Exception baseException = ce.GetBaseException();
            return (default(T), 0, $"Exception: Status code {ce.StatusCode}. Item ID: {id}.\nException details: {ce}");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
            return (default(T), 0, $"Exception: {e.Message}");
        }
    }
}