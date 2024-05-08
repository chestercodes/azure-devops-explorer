namespace AzureDevopsExplorer.Database.Model.Data;

public class SecurityNamespacePermission
{
    public int Id { get; set; }
    public Guid NamespaceId { get; set; }
    public int ActionBit { get; set; }
    public int Permission { get; set; }
}
