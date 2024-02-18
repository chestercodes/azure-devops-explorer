using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(PipelineRunId), nameof(ResourceRefName))]
public class PipelineRunPipelineInfo
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int PipelineRunId { get; set; }
    // ResourceRefName seen value up to 41
    [StringLength(128)]
    public string ResourceRefName { get; set; }
    // seen up to 71
    [StringLength(128)]
    public string Version { get; set; }
    // not worth storing and example is
    // https://dev.azure.com/someorg/proj-guid/_apis/pipelines/123?revision=3
    // public string? Url { get; set; }
    public int? Revision { get; set; }
    // Name is repo name, think the limit is 64 chars, double to be safe
    // https://learn.microsoft.com/en-us/azure/devops/organizations/settings/naming-restrictions?view=azure-devops#azure-repos-git
    [StringLength(128)]
    public string? Name { get; set; }
    // Folder seen value up to 176
    [StringLength(512)]
    public string? Folder { get; set; }
}
