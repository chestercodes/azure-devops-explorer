namespace AzureDevopsExplorer.Database.Model.Environment;

public class ServiceEndpointChange
{
    public int Id { get; set; }
    public DateTime? PreviousImport { get; set; }
    public DateTime? NextImport { get; set; }
    public Guid ServiceEndpointId { get; set; }
    public string Difference { get; set; }
}
