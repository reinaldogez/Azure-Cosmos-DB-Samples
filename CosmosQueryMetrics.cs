using System.Diagnostics;

namespace AzureCosmosDBSamples;

public class CosmosQueryMetrics
{
    private readonly CosmosQueryEngine _cosmosQueryEngine;

    public CosmosQueryMetrics(CosmosQueryEngine cosmosQueryEngine)
    {
        _cosmosQueryEngine = cosmosQueryEngine;
    }
    public async Task<List<T>> QueryMassiveQueryContainerPostByAuthor<T>(string author, string databaseName, string containerName)
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        var queryString = $"SELECT * FROM c WHERE c.Author = '{author}'";
        var (results, requestCharge) = await _cosmosQueryEngine.QueryItems<T>(queryString, databaseName, containerName);

        Console.WriteLine($"Total request charge QueryMassiveQueryContainerPostByAuthor: {requestCharge}");
        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;

        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        Console.WriteLine("\tTotal time QueryMassiveQueryContainerPostByAuthor: {0}", elapsedTime);
        return results;
    }

    public async Task<List<T>> QueryDefaultPostByAuthor<T>(string author, string databaseName, string containerName)
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        var queryString = $"SELECT * FROM c WHERE c.Author = '{author}'";
        var (results, requestCharge) = await _cosmosQueryEngine.QueryItems<T>(queryString, databaseName, containerName);

        Console.WriteLine($"Total request charge QueryDefaultPostByAuthor: {requestCharge}");

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;

        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        Console.WriteLine("\tTotal time QueryDefaultPostByAuthor: {0}", elapsedTime);
        return results;
    }

}