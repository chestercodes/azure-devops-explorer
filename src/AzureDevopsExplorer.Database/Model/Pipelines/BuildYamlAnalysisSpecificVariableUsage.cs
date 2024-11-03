using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

[PrimaryKey(nameof(FileHash), nameof(SpecificVariableRefId))]
public class BuildYamlAnalysisSpecificVariableUsage
{
    [StringLength(32)]
    public string FileHash { get; set; }
    public int SpecificVariableRefId { get; set; }
}
