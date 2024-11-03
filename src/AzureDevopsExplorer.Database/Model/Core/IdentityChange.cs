namespace AzureDevopsExplorer.Database.Model.Pipelines;

public class IdentityChange
{
    public int Id { get; set; }
    public DateTime? PreviousImport { get; set; }
    public DateTime? NextImport { get; set; }
    public Guid IdentityId { get; set; }
    public string Difference { get; set; }
}
