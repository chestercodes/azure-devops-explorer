using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class VariableGroup
{
    [JsonProperty("variables")]
    public Dictionary<string, Variable> Variables { get; set; } = new();
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("type")]
    public string? Type { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }
    [JsonProperty("description")]
    public string? Description { get; set; }
    [JsonProperty("createdBy")]
    public VariableGroupIdentity? CreatedBy { get; set; }
    [JsonProperty("createdOn")]
    public DateTime? CreatedOn { get; set; }
    [JsonProperty("modifiedBy")]
    public VariableGroupIdentity? ModifiedBy { get; set; }
    [JsonProperty("modifiedOn")]
    public DateTime? ModifiedOn { get; set; }
    [JsonProperty("isShared")]
    public bool? IsShared { get; set; }
    [JsonProperty("variableGroupProjectReferences")]
    public List<VariableGroupProjectReference>? VariableGroupProjectReferences { get; set; } = new();
}

public class Variable
{
    [JsonProperty("value")]
    public string? Value { get; set; }
    [JsonProperty("isSecret")]
    public bool? IsSecret { get; set; }
}

public class VariableGroupIdentity
{
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("uniqueName")]
    public string UniqueName { get; set; }
}

public class VariableGroupProjectReference
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("projectReference")]
    public VariableGroupProjectReferenceProject ProjectReference { get; set; }
}

public class VariableGroupProjectReferenceProject
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
}
