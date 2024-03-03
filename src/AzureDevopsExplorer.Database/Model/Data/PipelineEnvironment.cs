using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

public class PipelineEnvironment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid LastModifiedById { get; set; }
    public DateTime LastModifiedOn { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime? LastImport { get; set; }
}
