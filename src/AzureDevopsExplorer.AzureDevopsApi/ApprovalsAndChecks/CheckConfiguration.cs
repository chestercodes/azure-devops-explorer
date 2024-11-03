namespace AzureDevopsExplorer.AzureDevopsApi.ApprovalsAndChecks;

public class CheckConfiguration
{
    public object? settings { get; set; }
    public Createdby? createdBy { get; set; }
    public DateTime? createdOn { get; set; }
    public Modifiedby? modifiedBy { get; set; }
    public DateTime? modifiedOn { get; set; }
    public int? timeout { get; set; }
    public int id { get; set; }
    public int version { get; set; }
    public Type? type { get; set; }
    public string? url { get; set; }
    public Resource? resource { get; set; }
}

public class Createdby
{
    public string? displayName { get; set; }
    public string? id { get; set; }
    public string? uniqueName { get; set; }
    public string? descriptor { get; set; }
}

public class Modifiedby
{
    public string? displayName { get; set; }
    public string? id { get; set; }
    public string? uniqueName { get; set; }
    public string? descriptor { get; set; }
}

public class Type
{
    public string? id { get; set; }
    public string? name { get; set; }
}

public class Resource
{
    public string? type { get; set; }
    public string? id { get; set; }
    public string? name { get; set; }
}
