using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(BuildArtifactId), nameof(Name))]
public class BuildArtifactProperty
{
    public long BuildArtifactId { get; set; }
    // bit of a guess
    [StringLength(512)]
    public string Name { get; set; }
    // bit of a guess
    [StringLength(512)]
    public string? Value { get; set; }
}