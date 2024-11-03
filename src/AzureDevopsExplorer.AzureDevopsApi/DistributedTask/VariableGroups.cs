namespace AzureDevopsExplorer.AzureDevopsApi.DistributedTask;

public class VariableGroup
{
    public Dictionary<string, Variable> variables { get; set; } = new();
    public int id { get; set; }
    public string? type { get; set; }
    public string? name { get; set; }
    public string? description { get; set; }
    public VariableGroupIdentity? createdBy { get; set; }
    public DateTime? createdOn { get; set; }
    public VariableGroupIdentity? modifiedBy { get; set; }
    public DateTime? modifiedOn { get; set; }
    public bool? isShared { get; set; }
    public List<VariableGroupProjectReference>? variableGroupProjectReferences { get; set; } = new();
}

public class Variable
{
    public string? value { get; set; }
    public bool? isSecret { get; set; }
}

public class VariableGroupIdentity
{
    public string displayName { get; set; }
    public string id { get; set; }
    public string uniqueName { get; set; }
}

public class VariableGroupProjectReference
{
    public string name { get; set; }
    public VariableGroupProjectReferenceProject projectReference { get; set; }
}

public class VariableGroupProjectReferenceProject
{
    public string name { get; set; }
    public string id { get; set; }
}
