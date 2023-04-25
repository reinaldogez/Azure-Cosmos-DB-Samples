using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AzureCosmosDBSamples;
using Microsoft.Azure.Cosmos;

var builder = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

var configuration = builder.Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);

var cosmosDbSettings = new CosmosDbSettings();
configuration.Bind("CosmosDbSettings", cosmosDbSettings);

CosmosClientOptions cosmosClientOptions = new CosmosClientOptions
{
    AllowBulkExecution = true
};
services.AddSingleton(s => new CosmosClient(cosmosDbSettings.EndpointUri, cosmosDbSettings.PrimaryKey, cosmosClientOptions));
services.AddScoped<CosmosDBManager>();
services.AddScoped<CosmosBatchManager>();
services.AddScoped<CosmosQueryEngine>();
services.AddScoped<CosmosQueryMetrics>();
services.AddSingleton<CosmosDbSettings>(cosmosDbSettings);

var serviceProvider = services.BuildServiceProvider();

var cosmosDBManager = serviceProvider.GetRequiredService<CosmosDBManager>();
//await cosmosDBManager.CheckConnection();
// await cosmosDBManager.CheckDatabaseExists(cosmosDbSettings.DatabaseName);
//await cosmosDBManager.CreateDatabase(cosmosDbSettings.DatabaseName, throughput: 8000);

List<ContainerInfo> queryMassiveContainers = JsonUtils.GetContainersFromJsonFile("cosmosdb-containers-query-massive.json");
await cosmosDBManager.CreateContainersList(queryMassiveContainers, cosmosDbSettings.DatabaseName);
//List<CosmosPostEntity> listCosmosPostEntities = DatabaseSeeder.SeedPostByAuthorContainer();
//var cosmosBatchManager = serviceProvider.GetRequiredService<CosmosBatchManager>();
// await cosmosBatchManager.InsertBatchItemsAsync(cosmosDbSettings.DatabaseName, "PostByAuthor", listCosmosPostEntities, post => post.Author);

// List<CosmosPostEntity> listCosmosPostEntities2 = DatabaseSeeder.SeedPostByAuthorContainer();
// await cosmosDBManager.InsertBulkItemsAsync(cosmosDbSettings.DatabaseName, "PostByAuthor", listCosmosPostEntities2);

List<ContainerInfo> defaultContainers = JsonUtils.GetContainersFromJsonFile("cosmosdb-containers-default-settings.json");
await cosmosDBManager.CreateContainersList(defaultContainers, cosmosDbSettings.DatabaseName);
// List<CosmosPostEntity> listCosmosPostEntities3 = DatabaseSeeder.SeedPostByAuthorContainer();
// await cosmosDBManager.InsertBulkItemsAsync(cosmosDbSettings.DatabaseName, "Post", listCosmosPostEntities3);


var _cosmosQueryMetrics = serviceProvider.GetRequiredService<CosmosQueryMetrics>();

await QueryNumberOfRowsInContainers();
await ComparePerfomanceBetweenPostByAuthorAndPostContainers();
await ComparePerfomanceBetweenPostByAuthorAndPostContainers();

async Task<bool> ComparePerfomanceBetweenPostByAuthorAndPostContainers()
{
    Console.ForegroundColor = ConsoleColor.Green;
    List<CosmosPostEntity> QueryDefaultPostByAuthorResponse =
        await _cosmosQueryMetrics.GenericQuery<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Author = 'z'",
            cosmosDbSettings.DatabaseName,
            containerName: "Post",
            queryName: "Query container Post(First query in Post container, Author doesn't exist in database)"
        );

    List<CosmosPostEntity> QueryDefaultPostByAuthorResponse2 =
        await _cosmosQueryMetrics.GenericQuery<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Author = 'w'",
            cosmosDbSettings.DatabaseName,
            containerName: "Post",
            queryName: "Query container Post(Author doesn't exist in database)"
        );
    List<CosmosPostEntity> QueryDefaultPostByAuthorResponse3 =
        await _cosmosQueryMetrics.GenericQuery<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Author = 'Christa Howell'",
            cosmosDbSettings.DatabaseName,
            containerName: "Post",
            queryName: "Query container Post(Author exists in database)"
        );
    Console.ResetColor();
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.Magenta;
    List<CosmosPostEntity> QueryMassiveQueryContainerPostByAuthor =
        await _cosmosQueryMetrics.GenericQuery<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Author = 'x'",
            cosmosDbSettings.DatabaseName,
            containerName: "PostByAuthor",
            queryName: "Query container PostByAuthor(First query in PostByAuthor container, Author doesn't exist in database)"
        );

    List<CosmosPostEntity> QueryMassiveQueryContainerPostByAuthor2 =
        await _cosmosQueryMetrics.GenericQuery<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Author = 'y'",
            cosmosDbSettings.DatabaseName,
            containerName: "PostByAuthor",
            queryName: "Query container PostByAuthor(Author doesn't exist in database)"
        );

    List<CosmosPostEntity> QueryMassiveQueryContainerPostByAuthor3 =
        await _cosmosQueryMetrics.GenericQuery<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Author = 'Sydney Kiehn'",
            cosmosDbSettings.DatabaseName,
            containerName: "PostByAuthor",
            queryName: "Query container PostByAuthor(Author exists in database)"
        );
    Console.ResetColor();
    Console.WriteLine();

    List<PostEntity> postList = QueryMassiveQueryContainerPostByAuthor3.ConvertAll(c => (PostEntity)c);
    return true;
}

async Task<bool> QueryNumberOfRowsInContainers()
{
    long numberOfRowsInPostByAuthorContainer = await _cosmosQueryMetrics.GenericQuerySingleValueAsync<long>(
        query: "SELECT VALUE COUNT(1) FROM c",
        cosmosDbSettings.DatabaseName,
        containerName: "PostByAuthor",
        queryName: "Query number of rows in PostByAuthor container"
    );
    Console.WriteLine($"Number of rows in PostByAuthor container {string.Format("{0:N0}", numberOfRowsInPostByAuthorContainer)}");

    long numberOfRowsInPostContainer = await _cosmosQueryMetrics.GenericQuerySingleValueAsync<long>(
        query: "SELECT VALUE COUNT(1) FROM c",
        cosmosDbSettings.DatabaseName,
        containerName: "Post",
        queryName: "Query number of rows in Post container"
    );
    Console.WriteLine($"Number of rows in Post container {string.Format("{0:N0}", numberOfRowsInPostContainer)}");
    return true;
}