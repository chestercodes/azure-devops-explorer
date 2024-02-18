using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(BuildId), nameof(Name))]
public class BuildTriggerInfo
{
    public int BuildId { get; set; }
    // Name seen value up to 25
    [StringLength(128)]
    public string Name { get; set; }
    // Value seen value up to 186 (pr title)
    [StringLength(512)]
    public string? Value { get; set; }
}