using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

[PrimaryKey(nameof(BuildRunId))]
public class BuildImport
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int BuildRunId { get; set; }
    public int PipelineId { get; set; }
    public int? PipelineRevision { get; set; }
    public BuildImportState PipelineRunImportState { get; set; }
    [StringLength(32)]
    public string? PipelineRunImportErrorHash { get; set; }
    public BuildImportState ArtifactImportState { get; set; }
    [StringLength(32)]
    public string? ArtifactImportErrorHash { get; set; }
    public BuildImportState TimelineImportState { get; set; }
    [StringLength(32)]
    public string? TimelineImportErrorHash { get; set; }
}

public enum BuildImportState
{
    Initial,
    Done,
    ErrorFromApi
}
