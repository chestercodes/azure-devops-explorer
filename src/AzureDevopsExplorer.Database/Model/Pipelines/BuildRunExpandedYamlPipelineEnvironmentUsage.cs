using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

[PrimaryKey(nameof(FileHash), nameof(PipelineEnvironmentRefId))]
public class BuildRunExpandedYamlPipelineEnvironmentUsage
{
    [StringLength(32)]
    public string FileHash { get; set; }
    public int PipelineEnvironmentRefId { get; set; }
}
