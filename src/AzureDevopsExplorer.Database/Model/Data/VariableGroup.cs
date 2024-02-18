using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

public class VariableGroup
{
    public DateTime? LastImport { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public List<Variable> Variables { get; set; } = new();
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? CreatedById { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? ModifiedById { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public bool? IsShared { get; set; }
    public List<VariableGroupProjectReference> VariableGroupProjectReferences { get; set; } = new();
}
