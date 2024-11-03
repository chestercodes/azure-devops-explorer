namespace AzureDevopsExplorer.Database.Model.Security;

public class SecurityNamespaceAction
{
    public int Id { get; set; }
    public Guid NamespaceId { get; set; }
    public int Bit { get; set; }
    public string Name { get; set; }
    public string? DisplayName { get; set; }
}
