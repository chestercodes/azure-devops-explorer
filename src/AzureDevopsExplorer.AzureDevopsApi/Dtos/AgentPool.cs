using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class AgentPool
{
    [JsonProperty("createdOn")]
    public DateTime CreatedOn { get; set; }
    [JsonProperty("autoProvision")]
    public bool AutoProvision { get; set; }
    [JsonProperty("autoUpdate")]
    public bool AutoUpdate { get; set; }
    [JsonProperty("autoSize")]
    public bool AutoSize { get; set; }
    [JsonProperty("targetSize")]
    public int? TargetSize { get; set; }
    [JsonProperty("agentCloudId")]
    public int? AgentCloudId { get; set; }
    [JsonProperty("createdBy")]
    public Createdby CreatedBy { get; set; }
    [JsonProperty("owner")]
    public Owner Owner { get; set; }
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("scope")]
    public string Scope { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("isHosted")]
    public bool IsHosted { get; set; }
    [JsonProperty("poolType")]
    public string PoolType { get; set; }
    [JsonProperty("size")]
    public int Size { get; set; }
    [JsonProperty("isLegacy")]
    public bool IsLegacy { get; set; }
    [JsonProperty("options")]
    public string Options { get; set; }
}

public class AgentPoolCreatedby
{
    [JsonProperty("id")]
    public string Id { get; set; }
}

public class AgentPoolOwner
{
    [JsonProperty("id")]
    public string Id { get; set; }
}

