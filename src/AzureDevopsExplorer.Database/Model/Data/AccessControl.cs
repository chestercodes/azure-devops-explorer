namespace AzureDevopsExplorer.Database.Model.Data;

public class AccessControl
{
    public long Id { get; set; }
    public bool InheritPermissions { get; set; }
    public Guid NamespaceId { get; set; }
    public string Token { get; set; }
    public string Descriptor { get; set; }
    public int Allow { get; set; }
    public int Deny { get; set; }

    public DateTime? LastImport { get; set; }
}
