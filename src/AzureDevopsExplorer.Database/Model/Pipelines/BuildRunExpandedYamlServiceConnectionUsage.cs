using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

[PrimaryKey(nameof(FileHash), nameof(ServiceConnectionRefId))]
public class BuildRunExpandedYamlServiceConnectionUsage
{
    [StringLength(32)]
    public string FileHash { get; set; }
    public int ServiceConnectionRefId { get; set; }
}
