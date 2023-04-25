using Newtonsoft.Json;

namespace AzureCosmosDBSamples.Entities;

public class CosmosPostEntity : PostEntity
{
    [JsonProperty("id")]
    public string CosmosId => PostId.ToString();
}