using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Environment;

[PrimaryKey(nameof(ServiceEndpointId), nameof(Name))]
public class ServiceEndpointAuthorizationParameter
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid ServiceEndpointId { get; set; }
    public string? Name { get; set; }
    public string? Value { get; set; }
}
