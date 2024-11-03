namespace AzureDevopsExplorer.Database.Model.Pipelines;

public class PipelineCurrentChange
{
    public int Id { get; set; }
    public DateTime? PreviousImport { get; set; }
    public DateTime? NextImport { get; set; }
    public int PipelineId { get; set; }
    public string Difference { get; set; }
}
