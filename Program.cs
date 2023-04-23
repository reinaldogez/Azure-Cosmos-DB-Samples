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

// Register CosmosClient as a singleton instance
services.AddSingleton(s => new CosmosClient(cosmosDbSettings.EndpointUri, cosmosDbSettings.PrimaryKey));
services.AddScoped<CosmosDBManager>();
services.AddSingleton<CosmosDbSettings>(cosmosDbSettings);

var serviceProvider = services.BuildServiceProvider();

var cosmosDBManager = serviceProvider.GetRequiredService<CosmosDBManager>();
await cosmosDBManager.CheckConnection();
//await cosmosDBManager.CreateDatabase(cosmosDbSettings.DatabaseName, throughput: 800);
List<ContainerInfo> queryMassiveContainers = JsonUtils.GetContainersFromJsonFile("cosmosdb-containers-query-massive.json");
await cosmosDBManager.CreateContainersList(queryMassiveContainers, cosmosDbSettings.DatabaseName);
await cosmosDBManager.CheckDatabaseExists(cosmosDbSettings.DatabaseName);

