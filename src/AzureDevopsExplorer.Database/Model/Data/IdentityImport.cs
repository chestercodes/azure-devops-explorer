using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

public class IdentityImport
{
    public int Id { get; set; }
    public Guid? IdentityId { get; set; }
    public string? Descriptor { get; set; }
    public string? SubjectDescriptor { get; set; }
    public DateTime? LastImport { get; set; }
}
