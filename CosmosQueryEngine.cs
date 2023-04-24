using System.Net;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace AzureCosmosDBSamples;

public class CosmosQueryEngine
{
    private readonly CosmosClient _cosmosClient;

    public CosmosQueryEngine(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task<(List<T> Results, double RequestCharge)> QueryItems<T>(string queryString, string databaseName, string containerName)
    {
        var container = _cosmosClient.GetContainer(databaseName, containerName);
        var query = container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
        var results = new List<T>();

        double totalRequestCharge = 0;

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());

            totalRequestCharge += response.RequestCharge;
        }

        return (results, totalRequestCharge);
    }
}