using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AzureCosmosDBSamples;
using Microsoft.Azure.Cosmos;
using AzureCosmosDBSamples.Managers;
using AzureCosmosDBSamples.Entities;
using AzureCosmosDBSamples.Utils;


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
var _cosmosQueryMetrics = serviceProvider.GetRequiredService<CosmosQueryMetrics>();

try
{
    await ComparePerfomanceBetweenPostByAuthorAndPostContainerQueriedById("c859aa25-49a0-4e63-8a5e-fb42a331f47e");
    await Go();
}
catch (Exception e)
{
    Console.WriteLine("Error: {0}", e);
}
finally
{
    Console.WriteLine("End of Azure-Cosmos-DB-Samples, press any key to exit.");
    Console.ReadLine();
}

async Task Go()
{
    while (true)
    {
        PrintPrompt();

        ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
        switch (consoleKeyInfo.Key)
        {
            case ConsoleKey.D0:
                await CheckAndCreateDatabase();
                await CreateContainers();
                break;
            case ConsoleKey.D1:
                await SeedDatabase();
                break;
            case ConsoleKey.D2:
                await ComparePerformanceBetweenPostByAuthorAndPostContainers();
                break;
            case ConsoleKey.D3:
                await ComparePerformanceBetweenPostByAuthorPostByLikesAndPostContainersQueriedByNumberOfLikes(200);
                await ComparePerformanceBetweenPostByAuthorPostByLikesAndPostContainersQueriedByNumberOfLikes(500);
                await ComparePerformanceBetweenPostByAuthorPostByLikesAndPostContainersQueriedByNumberOfLikes(800);
                break;
            case ConsoleKey.D4:
                await ComparePerfomanceBetweenPostByAuthorAndPostContainerQueriedById("c859aa25-49a0-4e63-8a5e-fb42a331f47e");
                break;
            case ConsoleKey.D5:

                break;
            case ConsoleKey.D6:

                break;
            case ConsoleKey.D7:

                break;
            case ConsoleKey.D8:

                break;
            case ConsoleKey.Escape:
                Console.WriteLine("Exiting...");
                return;
            default:
                Console.WriteLine("Select choice");
                break;
        }
    }
}

void PrintPrompt()
{
    Console.WriteLine("0 - Scenario 0: Setup Cosmos DB and Containers");
    Console.WriteLine("1 - Scenario 1: Seed Database");
    Console.WriteLine("2 - Scenario 2: Compare perfomance between PostByAuthor and Post containers");
    Console.WriteLine("3 - Scenario 3: Compare perfomance between PostByAuthor, Post, and PostByLikes containers queried by number of likes");
    Console.WriteLine("4 - Scenario 4: Compare perfomance between PostByAuthor and Post containers queried by id");
}

async Task<bool> ComparePerformanceBetweenPostByAuthorAndPostContainers()
{
    await QueryNumberOfRowsInContainers();

    Console.ForegroundColor = ConsoleColor.Green;
    List<CosmosPostEntity> QueryDefaultPostByAuthorResponse =
        await _cosmosQueryMetrics.QueryItemsWithMetricsAsync<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Author = 'z'",
            databaseName: cosmosDbSettings.DatabaseName,
            containerName: "Post",
            queryName: "Query container Post(First query in Post container, Author doesn't exist in database)"
        );

    List<CosmosPostEntity> QueryDefaultPostByAuthorResponse2 =
        await _cosmosQueryMetrics.QueryItemsWithMetricsAsync<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Author = 'w'",
            databaseName: cosmosDbSettings.DatabaseName,
            containerName: "Post",
            queryName: "Query container Post(Author doesn't exist in database)"
        );
    List<CosmosPostEntity> QueryDefaultPostByAuthorResponse3 =
        await _cosmosQueryMetrics.QueryItemsWithMetricsAsync<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Author = 'Christa Howell'",
            databaseName: cosmosDbSettings.DatabaseName,
            containerName: "Post",
            queryName: "Query container Post(Author exists in database)"
        );
    Console.ResetColor();
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.Magenta;
    List<CosmosPostEntity> QueryMassiveQueryContainerPostByAuthor =
        await _cosmosQueryMetrics.QueryItemsWithMetricsAsync<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Author = 'x'",
            databaseName: cosmosDbSettings.DatabaseName,
            containerName: "PostByAuthor",
            queryName: "Query container PostByAuthor(First query in PostByAuthor container, Author doesn't exist in database)"
        );

    List<CosmosPostEntity> QueryMassiveQueryContainerPostByAuthor2 =
        await _cosmosQueryMetrics.QueryItemsWithMetricsAsync<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Author = 'y'",
            databaseName: cosmosDbSettings.DatabaseName,
            containerName: "PostByAuthor",
            queryName: "Query container PostByAuthor(Author doesn't exist in database)"
        );

    List<CosmosPostEntity> QueryMassiveQueryContainerPostByAuthor3 =
        await _cosmosQueryMetrics.QueryItemsWithMetricsAsync<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Author = 'Sydney Kiehn'",
            databaseName: cosmosDbSettings.DatabaseName,
            containerName: "PostByAuthor",
            queryName: "Query container PostByAuthor(Author exists in database)"
        );
    Console.ResetColor();
    Console.WriteLine();

    // How convert CosmosPostEntity to PostEntity
    List<PostEntity> postList = QueryMassiveQueryContainerPostByAuthor3.ConvertAll(c => (PostEntity)c);
    return true;
}

async Task<bool> ComparePerfomanceBetweenPostByAuthorAndPostContainerQueriedById(string id)
{
    Console.WriteLine("Starting ComparePerfomanceBetweenPostByAuthorAndPostContainerQueriedById");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine($"Id: {id}");
    CosmosPostEntity PostInInPostByAuthorContainer =
    await _cosmosQueryMetrics.QuerySingleValueWithMetricsAsync<CosmosPostEntity>(
        query: $"SELECT * FROM c WHERE c.id = '{id}'",
        databaseName: cosmosDbSettings.DatabaseName,
        containerName: "PostByAuthor",
        queryName: "Query by id in PostByAuthor container",
        populateIndexMetrics: false
    );

    CosmosPostEntity PostInDefaultPostContainer =
        await _cosmosQueryMetrics.QueryPointReadAsyncWithMetricsAsync<CosmosPostEntity>(
            id: id,
            partitionKeyValue: id,
            databaseName: cosmosDbSettings.DatabaseName,
            containerName: "Post",
            queryName: "Query by id in Post container (using Point read)"
        );

    Console.ResetColor();
    Console.WriteLine();
    return true;
}

async Task<bool> ComparePerformanceBetweenPostByAuthorPostByLikesAndPostContainersQueriedByNumberOfLikes(int numberOfLikes)
{
    Console.WriteLine("Starting ComparePerformanceBetweenPostByAuthorPostByLikesAndPostContainersQueriedByNumberOfLikes");
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine($"numberOfLikes: {numberOfLikes}");

    List<CosmosPostEntity> PostsByLikesInDefaultPostContainer =
        await _cosmosQueryMetrics.QueryItemsWithMetricsAsync<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Likes > {numberOfLikes}",
            databaseName: cosmosDbSettings.DatabaseName,
            containerName: "Post",
            queryName: "Query by number of likes in Post container (First query)",
            populateIndexMetrics: true
        );
    List<CosmosPostEntity> PostsByLikesInPostByAuthorContainer =
        await _cosmosQueryMetrics.QueryItemsWithMetricsAsync<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Likes > {numberOfLikes}",
            databaseName: cosmosDbSettings.DatabaseName,
            containerName: "PostByAuthor",
            queryName: "Query by number of likes in PostByAuthor container (First query)",
            populateIndexMetrics: true
        );
    List<CosmosPostEntity> PostsByLikesInPostByLikesContainer =
        await _cosmosQueryMetrics.QueryItemsWithMetricsAsync<CosmosPostEntity>(
            query: $"SELECT * FROM c WHERE c.Likes > {numberOfLikes}",
            databaseName: cosmosDbSettings.DatabaseName,
            containerName: "PostByLikes",
            queryName: "Query by number of likes in PostByLikes container (First query)",
            populateIndexMetrics: true
        );
    Console.ResetColor();
    Console.WriteLine();
    return true;
}

async Task<bool> QueryNumberOfRowsInContainers()
{
    long numberOfRowsInPostByAuthorContainer = await _cosmosQueryMetrics.QuerySingleValueWithMetricsAsync<long>(
        query: "SELECT VALUE COUNT(1) FROM c",
        cosmosDbSettings.DatabaseName,
        containerName: "PostByAuthor",
        queryName: "Query number of rows in PostByAuthor container"
    );
    Console.WriteLine($"Number of rows in PostByAuthor container {string.Format("{0:N0}", numberOfRowsInPostByAuthorContainer)}");

    long numberOfRowsInPostContainer = await _cosmosQueryMetrics.QuerySingleValueWithMetricsAsync<long>(
        query: "SELECT VALUE COUNT(1) FROM c",
        cosmosDbSettings.DatabaseName,
        containerName: "Post",
        queryName: "Query number of rows in Post container"
    );
    Console.WriteLine($"Number of rows in Post container {string.Format("{0:N0}", numberOfRowsInPostContainer)}");

    long numberOfRowsInPostByLikesContainer = await _cosmosQueryMetrics.QuerySingleValueWithMetricsAsync<long>(
        query: "SELECT VALUE COUNT(1) FROM c",
        cosmosDbSettings.DatabaseName,
        containerName: "PostByLikes",
        queryName: "Query number of rows in PostByLikes container"
    );
    Console.WriteLine($"Number of rows in Post container {string.Format("{0:N0}", numberOfRowsInPostByLikesContainer)}");
    return true;
}

async Task<bool> CheckAndCreateDatabase()
{
    await cosmosDBManager.CheckConnection();
    await cosmosDBManager.CheckDatabaseExists(cosmosDbSettings.DatabaseName);
    await cosmosDBManager.CreateDatabase(cosmosDbSettings.DatabaseName, throughput: 20000);
    return true;
}

async Task<bool> SeedDatabase()
{
    List<CosmosPostEntity> listCosmosPostEntities = DatabaseSeeder.SeedPostByAuthorContainer();

    List<Task> tasks = new List<Task>(3);

    tasks.Add(cosmosDBManager.InsertBulkItemsAsync(cosmosDbSettings.DatabaseName, "PostByAuthor", listCosmosPostEntities));
    tasks.Add(cosmosDBManager.InsertBulkItemsAsync(cosmosDbSettings.DatabaseName, "PostByLikes", listCosmosPostEntities));
    tasks.Add(cosmosDBManager.InsertBulkItemsAsync(cosmosDbSettings.DatabaseName, "Post", listCosmosPostEntities));

    await Task.WhenAll(tasks);

    return true;
}

async Task<bool> CreateContainers()
{
    List<ContainerInfo> queryMassiveContainers = JsonUtils.GetContainersFromJsonFile("..\\config\\cosmosdb-containers-query-massive.json");
    await cosmosDBManager.CreateContainersList(queryMassiveContainers, cosmosDbSettings.DatabaseName);

    List<ContainerInfo> defaultContainers = JsonUtils.GetContainersFromJsonFile("..\\config\\cosmosdb-containers-default-settings.json");
    await cosmosDBManager.CreateContainersList(defaultContainers, cosmosDbSettings.DatabaseName);
    return true;
}