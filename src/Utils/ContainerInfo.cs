namespace AzureCosmosDBSamples.Utils;

public class ContainerInfo
{
    public string Name { get; set; }
    public string PartitionKeyPath { get; set; }
    public int? Throughput { get; set; }
    public string IndexingMode { get; set; }
    public List<string> IncludedPaths { get; set; }
    public List<string> ExcludedPaths { get; set; }
       
}