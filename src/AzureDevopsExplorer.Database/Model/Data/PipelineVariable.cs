using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

public class PipelineVariable
{
    public int Id { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int PipelineId { get; set; }
    public int PipelineRevision { get; set; }
    public string Name { get; set; }
    public string? Value { get; set; }
    public bool? IsSecret { get; set; }
}
