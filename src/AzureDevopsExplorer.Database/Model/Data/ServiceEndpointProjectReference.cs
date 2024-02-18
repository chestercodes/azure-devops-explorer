using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(ServiceEndpointId), nameof(Name))]
public class ServiceEndpointProjectReference
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string ServiceEndpointId { get; set; }
    public string? ProjectReferenceId { get; set; }
    public string? ProjectReferenceName { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
