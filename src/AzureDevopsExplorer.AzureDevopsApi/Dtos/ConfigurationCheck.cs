using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class ConfigurationCheck
{
    [JsonProperty("settings")]
    public object? Settings { get; set; }
    [JsonProperty("createdBy")]
    public Createdby? CreatedBy { get; set; }
    [JsonProperty("createdOn")]
    public DateTime? CreatedOn { get; set; }
    [JsonProperty("modifiedBy")]
    public Modifiedby? ModifiedBy { get; set; }
    [JsonProperty("modifiedOn")]
    public DateTime? ModifiedOn { get; set; }
    [JsonProperty("timeout")]
    public int? Timeout { get; set; }
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("version")]
    public int Version { get; set; }
    [JsonProperty("type")]
    public Type? Type { get; set; }
    [JsonProperty("url")]
    public string? Url { get; set; }
    [JsonProperty("resource")]
    public Resource? Resource { get; set; }
}

public class Createdby
{
    [JsonProperty("displayName")]
    public string? DisplayName { get; set; }
    [JsonProperty("id")]
    public string? Id { get; set; }
    [JsonProperty("uniqueName")]
    public string? UniqueName { get; set; }
    [JsonProperty("descriptor")]
    public string? Descriptor { get; set; }
}

public class Modifiedby
{
    [JsonProperty("displayName")]
    public string? DisplayName { get; set; }
    [JsonProperty("id")]
    public string? Id { get; set; }
    [JsonProperty("uniqueName")]
    public string? UniqueName { get; set; }
    [JsonProperty("descriptor")]
    public string? Descriptor { get; set; }
}

public class Type
{
    [JsonProperty("id")]
    public string? Id { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }
}

public class Resource
{
    [JsonProperty("type")]
    public string? Type { get; set; }
    [JsonProperty("id")]
    public string? Id { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }
}
