namespace AzureDevopsExplorer.Database.Model.Security;

public class PipelineEnvironmentPipelinePermission
{
    public long Id { get; set; }
    public int PipelineEnvironmentId { get; set; }
    public int PipelineId { get; set; }
    public Guid ProjectId { get; set; }
    public bool Authorised { get; set; }
    public Guid AuthorisedById { get; set; }
    public DateTime AuthorisedOn { get; set; }

    public DateTime LastImport { get; set; }
}
