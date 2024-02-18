using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;


public class PipelineYaml
{
    [JsonProperty("_links")]
    public Dictionary<string, Link>? Links { get; set; }
    [JsonProperty("configuration")]
    public PipelineYamlConfiguration? Configuration { get; set; }
    [JsonProperty("url")]
    public string? Url { get; set; }
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("revision")]
    public int? Revision { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }
    [JsonProperty("folder")]
    public string? Folder { get; set; }
}


public class PipelineYamlConfiguration
{
    [JsonProperty("path")]
    public string? Path { get; set; }
    [JsonProperty("repository")]
    public Repository? Repository { get; set; }
    [JsonProperty("type")]
    public string? Type { get; set; }
}
