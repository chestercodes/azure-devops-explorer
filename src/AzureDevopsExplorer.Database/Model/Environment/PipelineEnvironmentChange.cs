namespace AzureDevopsExplorer.Database.Model.Environment;

public class PipelineEnvironmentChange
{
    public int Id { get; set; }
    public DateTime? PreviousImport { get; set; }
    public DateTime? NextImport { get; set; }
    public int PipelineEnvironmentId { get; set; }
    public string Difference { get; set; }
}
