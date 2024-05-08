namespace AzureDevopsExplorer.Database.Model.Data;

public class AgentPoolChange
{
    public int Id { get; set; }
    public DateTime? PreviousImport { get; set; }
    public DateTime? NextImport { get; set; }
    public int AgentPoolId { get; set; }
    public string Difference { get; set; }
}
