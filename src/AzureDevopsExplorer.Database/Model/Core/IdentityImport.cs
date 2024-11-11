using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

public class IdentityImport
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string SubjectDescriptor { get; set; }
    public DateTime? LastImport { get; set; }
}
