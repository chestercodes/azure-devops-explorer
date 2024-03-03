namespace AzureDevopsExplorer.Database.Model.Data;

public class CheckConfigurationChange
{
    public int Id { get; set; }
    public DateTime? PreviousImport { get; set; }
    public DateTime? NextImport { get; set; }
    public int CheckId { get; set; }
    public string ResourceId { get; set; }
    public string ResourceType { get; set; }
    public string ResourceName { get; set; }
    public string Difference { get; set; }
}
