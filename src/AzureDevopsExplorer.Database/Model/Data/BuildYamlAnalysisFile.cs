using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(Hash))]
public class BuildYamlAnalysisFile
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [StringLength(32)]
    public string Hash { get; set; }
    public string Content { get; set; }
    [StringLength(500)]
    public string? Description { get; set; }
    public BuildYamlAnalysisFileStatus Status { get; set; }
}

public enum BuildYamlAnalysisFileStatus
{
    NotProcessed,
    Failed,
    Ok
}