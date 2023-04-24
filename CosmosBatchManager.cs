using System.Net;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace AzureCosmosDBSamples;

public class CosmosBatchManager
{
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosDbSettings _cosmosDbSettings;

    public CosmosBatchManager(CosmosClient cosmosClient, CosmosDbSettings cosmosDbSettings)
    {
        _cosmosClient = cosmosClient;
        _cosmosDbSettings = cosmosDbSettings;
    }

    public async Task InsertBatchItemsAsync<T>(string databaseName, string containerName, List<T> items, Func<T, string> partitionKeySelector)
    {
        Console.WriteLine($"Starting batch insert of {items.Count} items into container {containerName}...");
        Container container = _cosmosClient.GetContainer(databaseName, containerName);

        var itemsGroupedByPartitionKey = items.GroupBy(partitionKeySelector);

        (double totalRequestCharge, int successCount, int failedCount, string elapsedTime) = await ExecuteBatchInsertAsync(container, itemsGroupedByPartitionKey);

        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("\tTotal time: {0}", elapsedTime);
        Console.WriteLine($"Batch insert of {items.Count} items into container {containerName} complete.");
        Console.WriteLine($"Total request charge: {totalRequestCharge}");
        Console.WriteLine($"Inserted items: {successCount}");
        Console.WriteLine($"Failed items: {failedCount}");
    }

    private async Task<(double totalRequestCharge, int successCount, int failedCount, string elapsedTime)>
        ExecuteBatchInsertAsync<T>(Container container, IEnumerable<IGrouping<string, T>> itemsGroupedByPartitionKey)
    {
        var batchProcessingResult = new BatchProcessingResult();

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        var tasks = new ConcurrentBag<Task>();

        int maxRetries = 5;

        Parallel.ForEach(itemsGroupedByPartitionKey, itemGroup =>
        {
            Task task = ProcessItemGroupAsync(container, itemGroup, maxRetries, batchProcessingResult);
            tasks.Add(task);
        });

        await Task.WhenAll(tasks.ToArray());

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;

        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        return (batchProcessingResult.TotalRequestCharge, batchProcessingResult.SuccessCount, batchProcessingResult.FailedCount, elapsedTime);
    }

    private class BatchProcessingResult
    {
        public double TotalRequestCharge { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
    }

    private async Task ProcessItemGroupAsync<T>(Container container, IGrouping<string, T> itemGroup, int maxRetries, BatchProcessingResult result)
    {
        string partitionKey = itemGroup.Key;
        int itemCount = 0;

        var batch = container.CreateTransactionalBatch(new PartitionKey(partitionKey));
        foreach (var item in itemGroup)
        {
            batch.CreateItem(item);
            itemCount++;

            if (itemCount % 100 == 0)
            {
                await ExecuteBatchWithRetriesAsync(container, batch, partitionKey, maxRetries, itemCount, result);
                batch = container.CreateTransactionalBatch(new PartitionKey(partitionKey));
                itemCount = 0;
            }
        }

        if (itemCount > 0)
        {
            await ExecuteBatchWithRetriesAsync(container, batch, partitionKey, maxRetries, itemCount, result);
        }
    }

    private async Task ExecuteBatchWithRetriesAsync(Container container, TransactionalBatch batch, string partitionKey, int maxRetries, int itemCount, BatchProcessingResult result)
    {
        int retries = 0;
        bool success = false;

        while (retries < maxRetries && !success)
        {
            try
            {
                var response = await batch.ExecuteAsync();

                if (response.IsSuccessStatusCode)
                {
                    lock (result)
                    {
                        result.TotalRequestCharge += response.RequestCharge;
                        result.SuccessCount += itemCount;
                    }
                    success = true;
                }
                else
                {
                    throw new CosmosException(response.ErrorMessage, response.StatusCode, 0, response.ActivityId, response.RequestCharge);
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await HandleRetryAsync(container, partitionKey, ex, ++retries, maxRetries, itemCount, result);
            }
            catch (CosmosException ex)
            {
                HandleException(container, partitionKey, ex, itemCount, result);
                break;
            }
            catch (Exception ex)
            {
                HandleException(container, partitionKey, ex, itemCount, result);
                break;
            }
        }
    }

    private async Task HandleRetryAsync(Container container, string partitionKey, CosmosException ex, int retries, int maxRetries, int failedItemsCount, BatchProcessingResult result)
    {
        double delayInSeconds = Math.Pow(2, retries);
        await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));

        if (retries >= maxRetries)
        {
            lock (result)
            {
                result.FailedCount += failedItemsCount;
            }
            Console.WriteLine($"CosmosException for partition key: {partitionKey}\n{ex.Message}");
        }
    }

    private void HandleException(Container container, string partitionKey, Exception ex, int failedItemsCount, BatchProcessingResult result)
    {
        lock (result)
        {
            result.FailedCount += failedItemsCount;
        }
        Console.WriteLine($"Error processing batch for partition key: {partitionKey}\n{ex.Message}");
    }

}