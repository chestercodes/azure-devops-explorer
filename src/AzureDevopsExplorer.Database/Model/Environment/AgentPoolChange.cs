namespace AzureDevopsExplorer.Database.Model.Environment;

public class AgentPoolChange
{
    public int Id { get; set; }
    public int AgentPoolId { get; set; }
    public DateTime? PreviousImport { get; set; }
    public DateTime? NextImport { get; set; }
    public string Difference { get; set; }
}
