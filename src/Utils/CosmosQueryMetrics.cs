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

    public async Task<List<T>> QueryItemsWithMetricsAsync<T>(string query, string databaseName, string containerName, string queryName, bool populateIndexMetrics = false)
    {
        long startTimestamp = Stopwatch.GetTimestamp();

        var (results, requestCharge) = await _cosmosQueryEngine.QueryItemsAsync<T>(query, databaseName, containerName, populateIndexMetrics);

        Console.WriteLine($"Total request charge {queryName}: {requestCharge}");

        long endTimestamp = Stopwatch.GetTimestamp();
        long timestampDifference = endTimestamp - startTimestamp;

        double elapsedTimeInMilliseconds = ((double)timestampDifference / Stopwatch.Frequency) * 1000;

        Console.WriteLine($"Total time {queryName}: {elapsedTimeInMilliseconds} milliseconds");

        return results;
    }

    public async Task<T> QuerySingleValueWithMetricsAsync<T>(
        string query, string databaseName, string containerName, string queryName, bool populateIndexMetrics = false)
    {
        long startTimestamp = Stopwatch.GetTimestamp();

        var (result, requestCharge, errorMessage) = await _cosmosQueryEngine.QuerySingleValueAsync<T>(query, databaseName, containerName, populateIndexMetrics);

        Console.WriteLine($"Total request charge {queryName}: {requestCharge}");

        long endTimestamp = Stopwatch.GetTimestamp();
        long timestampDifference = endTimestamp - startTimestamp;

        double elapsedTimeInMilliseconds = ((double)timestampDifference / Stopwatch.Frequency) * 1000;

        Console.WriteLine($"Total time {queryName}: {elapsedTimeInMilliseconds} milliseconds");

        return (result);
    }

    public async Task<T> QueryPointReadAsyncWithMetricsAsync<T>(
        string id, string partitionKeyValue, string databaseName, string containerName, string queryName)
    {
        long startTimestamp = Stopwatch.GetTimestamp();

        var (result, requestCharge, errorMessage) = await _cosmosQueryEngine.PointReadAsync<T>(id, partitionKeyValue, databaseName, containerName);

        Console.WriteLine($"Total request charge {queryName}: {requestCharge}");

        long endTimestamp = Stopwatch.GetTimestamp();
        long timestampDifference = endTimestamp - startTimestamp;

        double elapsedTimeInMilliseconds = ((double)timestampDifference / Stopwatch.Frequency) * 1000;

        Console.WriteLine($"Total time {queryName}: {elapsedTimeInMilliseconds} milliseconds");

        return (result);
    }    
}