namespace AzureDevopsExplorer.Database.Model.Pipelines;

public class IdentityImport
{
    public int Id { get; set; }
    public Guid? IdentityId { get; set; }
    public string? Descriptor { get; set; }
    public string? SubjectDescriptor { get; set; }
    public DateTime? LastImport { get; set; }
}
