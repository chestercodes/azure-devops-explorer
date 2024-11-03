using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

[PrimaryKey(nameof(PipelineRunId), nameof(ResourceRefName))]
public class PipelineRunRepositoryInfo
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int PipelineRunId { get; set; }
    // ResourceRefName seen value up to 91
    [StringLength(512)]
    public string ResourceRefName { get; set; }
    [StringLength(200)]
    public string? RepositoryId { get; set; }
    // RepositoryType
    [StringLength(50)]
    public string? RepositoryType { get; set; }
    // RefName seen value up to 80
    [StringLength(512)]
    public string? RefName { get; set; }
    // Version is git hash
    [StringLength(40)]
    public string? Version { get; set; }
}
