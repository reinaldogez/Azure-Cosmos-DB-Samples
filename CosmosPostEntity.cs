using Newtonsoft.Json;

namespace AzureCosmosDBSamples;

public class CosmosPostEntity : PostEntity
{
    [JsonProperty("id")]
    public string CosmosId => PostId.ToString();
}