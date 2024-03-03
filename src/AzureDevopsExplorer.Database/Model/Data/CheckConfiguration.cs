using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(Id))]
public class CheckConfiguration
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public DateTime? LastImport { get; set; }
    public string? Settings { get; set; }
    public Guid? CreatedById { get; set; }
    public DateTime? CreatedOn { get; set; }
    public Guid? ModifiedById { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public int? Timeout { get; set; }
    public int Version { get; set; }
    public string? TypeId { get; set; }
    public string? TypeName { get; set; }
    public string? Url { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? ResourceName { get; set; }
}
