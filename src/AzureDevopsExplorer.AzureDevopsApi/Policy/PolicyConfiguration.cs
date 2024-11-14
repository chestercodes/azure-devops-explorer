namespace AzureDevopsExplorer.AzureDevopsApi.Policy;

public class PolicyConfiguration
{
    public Createdby createdBy { get; set; }
    public DateTime createdDate { get; set; }
    public bool isEnabled { get; set; }
    public bool isBlocking { get; set; }
    public bool isDeleted { get; set; }
    public Dictionary<string, object> settings { get; set; }
    public bool isEnterpriseManaged { get; set; }
    public int revision { get; set; }
    public int id { get; set; }
    public string url { get; set; }
    public Type type { get; set; }
}

public class Createdby
{
    public string? displayName { get; set; }
    public string? id { get; set; }
    public string uniqueName { get; set; }
    public string? imageUrl { get; set; }
    public string descriptor { get; set; }
}


public class Scope
{
    public string? repositoryId { get; set; }
    public string? refName { get; set; }
    public string? matchKind { get; set; }
}

public class Type
{
    public string? id { get; set; }
    public string? url { get; set; }
    public string? displayName { get; set; }
}
