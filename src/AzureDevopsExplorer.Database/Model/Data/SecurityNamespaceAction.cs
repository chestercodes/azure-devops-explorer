namespace AzureDevopsExplorer.Database.Model.Data;

public class SecurityNamespaceAction
{
    public int Id { get; set; }
    public Guid NamespaceId { get; set; }
    public int Bit { get; set; }
    public string Name { get; set; }
    public string? DisplayName { get; set; }
}
