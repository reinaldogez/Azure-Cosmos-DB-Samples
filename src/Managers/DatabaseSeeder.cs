using Bogus;
using AzureCosmosDBSamples.Entities;


namespace AzureCosmosDBSamples.Managers;

public static class DatabaseSeeder
{

    public static List<CosmosPostEntity> SeedPostByAuthorContainer()
    {

        try
        {
            int numberOfAuthors = 10_000;
            int minPostsPerAuthor = 2;
            int maxPostsPerAuthor = 20;

            Console.WriteLine($"Creating List<CosmosPostEntity>: " +
                              $"numberOfAuthors {numberOfAuthors} " +
                              $"minPostsPerAuthor{minPostsPerAuthor} " +
                              $"maxPostsPerAuthor{maxPostsPerAuthor}");

            var authorFaker = new Faker();
            var authors = new HashSet<string>();

            while (authors.Count < numberOfAuthors)
            {
                authors.Add(authorFaker.Name.FullName());
            }

            var postFaker = new Faker<CosmosPostEntity>()
                .RuleFor(u => u.PostId, f => Guid.NewGuid())
                .RuleFor(u => u.DatePosted, f => f.Date.Recent(30))
                .RuleFor(u => u.Message, f => f.Lorem.Sentence(5))
                .RuleFor(u => u.Likes, f => f.Random.Number(0, 1000));

            var posts = new List<CosmosPostEntity>();

            foreach (var author in authors)
            {
                int numberOfPosts = authorFaker.Random.Number(minPostsPerAuthor, maxPostsPerAuthor);
                var authorPosts = postFaker.Clone().RuleFor(u => u.Author, (f, u) => author).Generate(numberOfPosts);
                posts.AddRange(authorPosts);
            }
            return posts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
            return null;
        }
    }

}