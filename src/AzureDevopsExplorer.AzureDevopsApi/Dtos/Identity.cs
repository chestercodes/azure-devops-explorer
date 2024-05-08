using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class Identity
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("descriptor")]
    public string Descriptor { get; set; }
    [JsonPropertyName("subjectDescriptor")]
    public string SubjectDescriptor { get; set; }
    [JsonPropertyName("providerDisplayName")]
    public string? ProviderDisplayName { get; set; }
    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }
    [JsonPropertyName("isContainer")]
    public bool? IsContainer { get; set; }
    [JsonPropertyName("members")]
    public string[]? Members { get; set; }
    [JsonPropertyName("memberOf")]
    public string[]? MemberOf { get; set; }
    [JsonPropertyName("memberIds")]
    public string[]? MemberIds { get; set; }
    [JsonPropertyName("properties")]
    public Dictionary<string, IdentityPreviewProperty>? Properties { get; set; }
    [JsonPropertyName("resourceVersion")]
    public int? ResourceVersion { get; set; }
    [JsonPropertyName("metaTypeId")]
    public int? MetaTypeId { get; set; }
    [JsonPropertyName("customDisplayName")]
    public string? CustomDisplayName { get; set; }
}

public class IdentityPreviewProperty
{
    [JsonPropertyName("$type")]
    public string Type { get; set; }
    [JsonPropertyName("$value")]
    public string Value { get; set; }
}
