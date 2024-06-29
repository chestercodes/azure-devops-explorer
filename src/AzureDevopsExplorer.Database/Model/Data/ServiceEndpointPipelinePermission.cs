namespace AzureDevopsExplorer.Database.Model.Data;

public class ServiceEndpointPipelinePermission
{
    public long Id { get; set; }
    public Guid ServiceEndpointId { get; set; }
    public int PipelineId { get; set; }
    public Guid ProjectId { get; set; }
    public bool Authorised { get; set; }
    public Guid AuthorisedById { get; set; }
    public DateTime AuthorisedOn { get; set; }

    public DateTime LastImport { get; set; }
}
