namespace AzureCosmosDBSamples;

public class ContainerInfo
{
    public string Id { get; set; }
    public string PartitionKeyPath { get; set; }
    public int? Throughput { get; set; }
    public string IndexingMode { get; set; }
    public List<string> IncludedPaths { get; set; }
    public List<string> ExcludedPaths { get; set; }
       
}