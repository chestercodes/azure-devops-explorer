using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

[PrimaryKey(nameof(FileHash), nameof(VariableGroupRefId))]
public class BuildRunExpandedYamlVariableGroupUsage
{
    [StringLength(32)]
    public string FileHash { get; set; }
    public int VariableGroupRefId { get; set; }
}
