using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Environment;

[PrimaryKey(nameof(ServiceEndpointId), nameof(Name))]
public class ServiceEndpointProjectReference
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid ServiceEndpointId { get; set; }
    public Guid ProjectReferenceId { get; set; }
    public string? ProjectReferenceName { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
