using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Environment;

[PrimaryKey(nameof(Id))]
public class ServiceEndpoint
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    //public List<ServiceEndpointData>? Data { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Url { get; set; }
    public string? CreatedByDisplayName { get; set; }
    public string? CreatedByUrl { get; set; }
    public string CreatedById { get; set; }
    public string? CreatedByUniqueName { get; set; }
    public string? CreatedByImageUrl { get; set; }
    public string? CreatedByDescriptor { get; set; }
    public string? Description { get; set; }
    public string? AuthorizationScheme { get; set; }
    //public List<ServiceEndpointAuthorizationParameter>? AuthorizationParameters { get; set; }
    public bool? IsShared { get; set; }
    public bool? IsOutdated { get; set; }
    public bool? IsReady { get; set; }
    public string? Owner { get; set; }
    //public List<ServiceEndpointProjectReference>? ServiceEndpointProjectReferences { get; set; }
    public string? OperationStatusState { get; set; }
    public string? OperationStatusStatusMessage { get; set; }
    public DateTime? LastImport { get; set; }
}
