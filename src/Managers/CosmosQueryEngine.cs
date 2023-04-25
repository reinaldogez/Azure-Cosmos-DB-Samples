using Microsoft.Azure.Cosmos;

namespace AzureCosmosDBSamples.Managers;

public class CosmosQueryEngine
{
    private readonly CosmosClient _cosmosClient;

    public CosmosQueryEngine(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task<(List<T> Results, double RequestCharge)> QueryItems<T>(
        string queryString, string databaseName, string containerName, bool populateIndexMetrics = false)
    {
        var container = _cosmosClient.GetContainer(databaseName, containerName);
        var query = container.GetItemQueryIterator<T>(
            new QueryDefinition(queryString), requestOptions: new QueryRequestOptions
            {
                PopulateIndexMetrics = populateIndexMetrics,
                MaxItemCount = -1,
            });

        var results = new List<T>();

        double totalRequestCharge = 0;

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            if (populateIndexMetrics)
                Console.WriteLine("Index Metrics: " + response.IndexMetrics);
            results.AddRange(response.ToList());

            totalRequestCharge += response.RequestCharge;
        }

        return (results, totalRequestCharge);
    }

    public async Task<(T Value, double RequestCharge)> QuerySingleValueAsync<T>(
        string queryString, string databaseName, string containerName, bool populateIndexMetrics = false)
    {
        var container = _cosmosClient.GetContainer(databaseName, containerName);
        var query = container.GetItemQueryIterator<T>(
            new QueryDefinition(queryString), requestOptions: new QueryRequestOptions
            {
                PopulateIndexMetrics = populateIndexMetrics,
                MaxItemCount = -1,
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

        return (result, totalRequestCharge);
    }
}