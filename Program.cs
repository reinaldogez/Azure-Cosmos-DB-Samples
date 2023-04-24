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
services.AddSingleton<CosmosDbSettings>(cosmosDbSettings);

var serviceProvider = services.BuildServiceProvider();

var cosmosDBManager = serviceProvider.GetRequiredService<CosmosDBManager>();
//await cosmosDBManager.CheckConnection();
await cosmosDBManager.CreateDatabase(cosmosDbSettings.DatabaseName, throughput: 8000);

List<ContainerInfo> queryMassiveContainers = JsonUtils.GetContainersFromJsonFile("cosmosdb-containers-query-massive.json");
await cosmosDBManager.CreateContainersList(queryMassiveContainers, cosmosDbSettings.DatabaseName);

List<CosmosPostEntity> listCosmosPostEntities2 = DatabaseSeeder.SeedPostByAuthorContainer();
// await cosmosDBManager.InsertBulkItemsAsync(cosmosDbSettings.DatabaseName, "PostByAuthor", listCosmosPostEntities2);

List<CosmosPostEntity> listCosmosPostEntities = DatabaseSeeder.SeedPostByAuthorContainer();
List<PostEntity> postList = listCosmosPostEntities.ConvertAll(c => (PostEntity)c);
var cosmosBatchManager = serviceProvider.GetRequiredService<CosmosBatchManager>();

await cosmosBatchManager.InsertBatchItemsAsync(cosmosDbSettings.DatabaseName, "PostByAuthor", listCosmosPostEntities, post => post.Author);



// await cosmosDBManager.CheckDatabaseExists(cosmosDbSettings.DatabaseName);

