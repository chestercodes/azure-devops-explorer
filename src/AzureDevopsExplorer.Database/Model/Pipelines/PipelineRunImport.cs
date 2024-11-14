using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

[PrimaryKey(nameof(BuildRunId))]
public class PipelineRunImport
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int BuildRunId { get; set; }
    public int PipelineId { get; set; }
    public int? PipelineRevision { get; set; }
    public PipelineRunImportState PipelineRunImportState { get; set; }
    [StringLength(32)]
    public string? PipelineRunImportErrorHash { get; set; }
}

public enum PipelineRunImportState
{
    Initial,
    Done,
    ErrorFromApi
}
