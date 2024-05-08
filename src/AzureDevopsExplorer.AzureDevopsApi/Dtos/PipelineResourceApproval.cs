using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class PipelineResourceApproval
{
    [JsonProperty("allPipelines")]
    public PipelineResourceApprovalAllpipelines? AllPipelines { get; set; }
    [JsonProperty("resource")]
    public PipelineResourceApprovalResource? Resource { get; set; }
    [JsonProperty("pipelines")]
    public PipelineResourceApprovalPipeline[]? Pipelines { get; set; }
}

public class PipelineResourceApprovalResource
{
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
}

public class PipelineResourceApprovalPipeline
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("authorized")]
    public bool Authorized { get; set; }
    [JsonProperty("authorizedBy")]
    public PipelineResourceApprovalAuthorizedby AuthorizedBy { get; set; }
    [JsonProperty("authorizedOn")]
    public DateTime AuthorizedOn { get; set; }
}

public class PipelineResourceApprovalAuthorizedby
{
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("uniqueName")]
    public string UniqueName { get; set; }
    [JsonProperty("descriptor")]
    public string Descriptor { get; set; }
}

public class PipelineResourceApprovalAllpipelines
{
    [JsonProperty("authorized")]
    public bool Authorized { get; set; }
    [JsonProperty("authorizedBy")]
    public PipelineResourceApprovalAuthorizedby AuthorizedBy { get; set; }
    [JsonProperty("authorizedOn")]
    public DateTime AuthorizedOn { get; set; }
}
