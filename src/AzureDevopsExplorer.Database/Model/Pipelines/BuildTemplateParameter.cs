using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

[PrimaryKey(nameof(BuildId), nameof(Name))]
public class BuildTemplateParameter
{
    public int BuildId { get; set; }
    // bit of a guess
    [StringLength(512)]
    public string Name { get; set; }
    // if passing data into template, Value can be large
    public string? Value { get; set; }
}