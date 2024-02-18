using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class ServiceEndpoint
{
    [JsonProperty("data")]
    public Dictionary<string, string>? Data { get; set; }
    [JsonProperty("id")]
    public string? Id { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }
    [JsonProperty("type")]
    public string? Type { get; set; }
    [JsonProperty("url")]
    public string? Url { get; set; }
    [JsonProperty("createdBy")]
    public ServiceEndpointIdentityRef? CreatedBy { get; set; }
    [JsonProperty("description")]
    public string? Description { get; set; }
    [JsonProperty("authorization")]
    public Authorization? Authorization { get; set; }
    [JsonProperty("isShared")]
    public bool? IsShared { get; set; }
    [JsonProperty("isOutdated")]
    public bool? IsOutdated { get; set; }
    [JsonProperty("isReady")]
    public bool? IsReady { get; set; }
    [JsonProperty("owner")]
    public string? Owner { get; set; }
    [JsonProperty("serviceEndpointProjectReferences")]
    public Serviceendpointprojectreference[]? ServiceEndpointProjectReferences { get; set; }
    [JsonProperty("operationStatus")]
    public Operationstatus? OperationStatus { get; set; }
}

public class ServiceEndpointIdentityRef
{
    [JsonProperty("displayName")]
    public string? DisplayName { get; set; }
    [JsonProperty("url")]
    public string? Url { get; set; }
    //[JsonProperty("XXXXXXX")]
    //public _Links _links { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("uniqueName")]
    public string? UniqueName { get; set; }
    [JsonProperty("imageUrl")]
    public string? ImageUrl { get; set; }
    [JsonProperty("descriptor")]
    public string? Descriptor { get; set; }
}

//public class _Links
//{
//    public Avatar avatar { get; set; }
//}

//public class Avatar
//{
//    public string href { get; set; }
//}

public class Authorization
{
    [JsonProperty("parameters")]
    public Dictionary<string, string>? Parameters { get; set; }
    [JsonProperty("scheme")]
    public string? Scheme { get; set; }
}

public class Operationstatus
{
    [JsonProperty("state")]
    public string State { get; set; }
    [JsonProperty("statusMessage")]
    public string StatusMessage { get; set; }
}

public class Serviceendpointprojectreference
{
    [JsonProperty("projectReference")]
    public Projectreference? ProjectReference { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }
    [JsonProperty("description")]
    public string? Description { get; set; }
}

public class Projectreference
{
    [JsonProperty("id")]
    public string? Id { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }
}
