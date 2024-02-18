using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class PipelineRun
{
    [JsonProperty("_links")]
    public Dictionary<string, Link> Links { get; set; }
    [JsonProperty("templateParameters")]
    public Dictionary<string, string>? TemplateParameters { get; set; }
    [JsonProperty("pipeline")]
    public PipelineInfo Pipeline { get; set; }
    [JsonProperty("state")]
    public string State { get; set; }
    [JsonProperty("result")]
    public string? Result { get; set; }
    [JsonProperty("createdDate")]
    public DateTime CreatedDate { get; set; }
    [JsonProperty("finishedDate")]
    public DateTime? FinishedDate { get; set; }
    [JsonProperty("url")]
    public string? Url { get; set; }
    [JsonProperty("resources")]
    public Resources? Resources { get; set; }
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}

public class Link
{
    [JsonProperty("href")]
    public string Href { get; set; }
}

public class Resources
{
    [JsonProperty("repositories")]
    public Dictionary<string, RepositoryInfo>? Repositories { get; set; }
    [JsonProperty("pipelines")]
    public Dictionary<string, PipelineObj>? Pipelines { get; set; }
}

public class RepositoryInfo
{
    [JsonProperty("repository")]
    public Repository Repository { get; set; }
    [JsonProperty("refName")]
    public string RefName { get; set; }
    [JsonProperty("version")]
    public string Version { get; set; }
}

public class Repository
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("type")]
    public string Type { get; set; }
}

public class PipelineObj
{
    [JsonProperty("pipeline")]
    public PipelineInfo Pipeline { get; set; }
    [JsonProperty("version")]
    public string Version { get; set; }
}

public class PipelineInfo
{
    [JsonProperty("url")]
    public string Url { get; set; }
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("revision")]
    public int Revision { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("folder")]
    public string Folder { get; set; }
}

public enum RunResult
{
    [EnumMember(Value = "unknown")]
    Unknown,
    [EnumMember(Value = "succeeded")]
    Succeeded,
    [EnumMember(Value = "failed")]
    Failed,
    [EnumMember(Value = "canceled")]
    Canceled
}

public enum RunState
{
    [EnumMember(Value = "unknown")]
    Unknown,
    [EnumMember(Value = "inProgress")]
    InProgress,
    [EnumMember(Value = "canceling")]
    Canceling,
    [EnumMember(Value = "completed")]
    Completed
}