using System.Text.Json.Serialization;

namespace AzureDevopsExplorer.AzureDevopsApi.Core;

public class Identity
{
    public string id { get; set; }
    public string descriptor { get; set; }
    public string subjectDescriptor { get; set; }
    public string? providerDisplayName { get; set; }
    public bool? isActive { get; set; }
    public bool? isContainer { get; set; }
    public string[]? members { get; set; }
    public string[]? memberOf { get; set; }
    public string[]? memberIds { get; set; }
    public Dictionary<string, IdentityPreviewProperty>? properties { get; set; }
    public int? resourceVersion { get; set; }
    public int? metaTypeId { get; set; }
    public string? customDisplayName { get; set; }
}

public class IdentityPreviewProperty
{
    [JsonPropertyName("$type")]
    public string type { get; set; }
    [JsonPropertyName("$value")]
    public string value { get; set; }
}
