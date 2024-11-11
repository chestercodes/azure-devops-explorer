namespace AzureDevopsExplorer.Database.Model.Security;

public class PolicyConfigurationChange
{
    public int Id { get; set; }
    public DateTime? PreviousImport { get; set; }
    public DateTime? NextImport { get; set; }
    public int PolicyId { get; set; }
    public string Difference { get; set; }
}
