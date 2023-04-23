using Newtonsoft.Json;

namespace AzureCosmosDBSamples;

public static class JsonUtils
{
    public static List<ContainerInfo> GetContainersFromJsonFile(string filePath)
    {
        List<ContainerInfo> containers = new List<ContainerInfo>();

        try
        {
            string jsonContent = File.ReadAllText(filePath);
            containers = JsonConvert.DeserializeObject<List<ContainerInfo>>(jsonContent);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error reading file '{filePath}': {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error parsing JSON file '{filePath}': {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting containers from file '{filePath}': {ex.Message}");
        }

        return containers;
    }
}
