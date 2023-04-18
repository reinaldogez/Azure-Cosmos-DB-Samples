using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AzureCosmosDBSamples;

var builder = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

var configuration = builder.Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);

var serviceProvider = services.BuildServiceProvider();

CosmosDBManager cosmosDBManager = new CosmosDBManager(configuration);

await cosmosDBManager.CheckConnection();
await cosmosDBManager.CreateDatabase();
//cosmosDBManager.MongoDBConnection();
