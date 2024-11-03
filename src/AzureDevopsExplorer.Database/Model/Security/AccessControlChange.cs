namespace AzureDevopsExplorer.Database.Model.Security;

public class AccessControlChange
{
    public int Id { get; set; }
    public DateTime? PreviousImport { get; set; }
    public DateTime? NextImport { get; set; }
    public Guid NamespaceId { get; set; }
    public string Difference { get; set; }
}
