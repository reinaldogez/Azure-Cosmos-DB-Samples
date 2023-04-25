using System.Diagnostics;
using AzureCosmosDBSamples.Managers;

namespace AzureCosmosDBSamples.Utils;

public class CosmosQueryMetrics
{
    private readonly CosmosQueryEngine _cosmosQueryEngine;

    public CosmosQueryMetrics(CosmosQueryEngine cosmosQueryEngine)
    {
        _cosmosQueryEngine = cosmosQueryEngine;
    }

    public async Task<List<T>> GenericQuery<T>(string query, string databaseName, string containerName, string queryName, bool populateIndexMetrics = false)
    {
        long startTimestamp = Stopwatch.GetTimestamp();

        var (results, requestCharge) = await _cosmosQueryEngine.QueryItems<T>(query, databaseName, containerName);

        Console.WriteLine($"Total request charge {queryName}: {requestCharge}");

        long endTimestamp = Stopwatch.GetTimestamp();
        long timestampDifference = endTimestamp - startTimestamp;

        double elapsedTimeInMilliseconds = ((double)timestampDifference / Stopwatch.Frequency) * 1000;

        Console.WriteLine($"Total time {queryName}: {elapsedTimeInMilliseconds} milliseconds");

        return results;
    }

    public async Task<T> GenericQuerySingleValueAsync<T>(
        string query, string databaseName, string containerName, string queryName, bool populateIndexMetrics = false)
    {
        long startTimestamp = Stopwatch.GetTimestamp();

        var (result, requestCharge) = await _cosmosQueryEngine.QuerySingleValueAsync<T>(query, databaseName, containerName, populateIndexMetrics);

        Console.WriteLine($"Total request charge {queryName}: {requestCharge}");

        long endTimestamp = Stopwatch.GetTimestamp();
        long timestampDifference = endTimestamp - startTimestamp;

        double elapsedTimeInMilliseconds = ((double)timestampDifference / Stopwatch.Frequency) * 1000;

        Console.WriteLine($"Total time {queryName}: {elapsedTimeInMilliseconds} milliseconds");

        return (result);
    }
}