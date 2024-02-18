using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;


public class PipelineRef
{
    [JsonProperty("_links")]
    public Dictionary<string, Link> Links { get; set; }
    [JsonProperty("url")]
    public string? Url { get; set; }
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("revision")]
    public int Revision { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("folder")]
    public string Folder { get; set; }
}

