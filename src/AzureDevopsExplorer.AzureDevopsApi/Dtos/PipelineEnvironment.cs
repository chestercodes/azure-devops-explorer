using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class PipelineEnvironment
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("createdBy")]
    public PipelineEnvironmentIdentity CreatedBy { get; set; }
    [JsonProperty("createdOn")]
    public DateTime CreatedOn { get; set; }
    [JsonProperty("lastModifiedBy")]
    public PipelineEnvironmentIdentity LastModifiedBy { get; set; }
    [JsonProperty("lastModifiedOn")]
    public DateTime LastModifiedOn { get; set; }
    [JsonProperty("project")]
    public PipelineEnvironmentProject Project { get; set; }
}

public class PipelineEnvironmentIdentity
{
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
}

public class PipelineEnvironmentProject
{
    [JsonProperty("id")]
    public string Id { get; set; }
}
