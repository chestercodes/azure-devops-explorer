using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(PipelineId), nameof(PipelineRevision))]
public class PipelineImport
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int PipelineId { get; set; }
    public int PipelineRevision { get; set; }
    public PipelineImportState PipelineImportState { get; set; }
    [StringLength(32)]
    public string? PipelineImportErrorHash { get; set; }
}

public enum PipelineImportState
{
    Initial,
    Done,
    ErrorFromApi
}
