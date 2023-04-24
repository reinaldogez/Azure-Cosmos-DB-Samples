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

List<CosmosPostEntity> listCosmosPostEntitiesQueryResponse2 = await 
    _cosmosQueryMetrics.QueryDefaultPostByAuthor<CosmosPostEntity>(
        "x", 
        cosmosDbSettings.DatabaseName, 
        "Post"
    );

List<CosmosPostEntity> listCosmosPostEntitiesQueryResponse3 = await 
    _cosmosQueryMetrics.QueryDefaultPostByAuthor<CosmosPostEntity>(
        "x", 
        cosmosDbSettings.DatabaseName, 
        "Post"
    );


List<CosmosPostEntity> listCosmosPostEntitiesQueryResponse = await 
    _cosmosQueryMetrics.QueryMassiveQueryContainerPostByAuthor<CosmosPostEntity>(
        "x", 
        cosmosDbSettings.DatabaseName, 
        "PostByAuthor"
    );


List<PostEntity> postList = listCosmosPostEntitiesQueryResponse.ConvertAll(c => (PostEntity)c);


