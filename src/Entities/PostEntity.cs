namespace AzureCosmosDBSamples.Entities;

public class PostEntity
{
    public Guid PostId { get; set; }
    public string Author { get; set; }
    public DateTime DatePosted { get; set; }
    public string Message { get; set; }
    public int Likes { get; set; }
}