using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(FileHash), nameof(VariableGroupRefId))]
public class BuildYamlAnalysisVariableGroupUsage
{
    [StringLength(32)]
    public string FileHash { get; set; }
    public int VariableGroupRefId { get; set; }
}
