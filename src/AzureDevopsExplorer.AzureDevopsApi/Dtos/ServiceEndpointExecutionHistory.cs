
using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class ServiceEndpointExecutionHistory
{
    [JsonProperty("endpointId")]
    public string EndpointId { get; set; }
    [JsonProperty("data")]
    public Data Data { get; set; }
}

public class Data
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("planType")]
    public string PlanType { get; set; }
    [JsonProperty("definition")]
    public Definition Definition { get; set; }
    [JsonProperty("owner")]
    public Owner Owner { get; set; }
    [JsonProperty("startTime")]
    public DateTime StartTime { get; set; }
    [JsonProperty("finishTime")]
    public DateTime? FinishTime { get; set; }
    [JsonProperty("result")]
    public string Result { get; set; }
    [JsonProperty("ownerDetails")]
    public string OwnerDetails { get; set; }
}

public class Definition
{
    [JsonProperty("_links")]
    public Dictionary<string, Link>? Links { get; set; }
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}
public class Owner
{
    [JsonProperty("_links")]
    public Dictionary<string, Link>? Links { get; set; }
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}
