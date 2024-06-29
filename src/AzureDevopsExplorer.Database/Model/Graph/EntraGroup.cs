using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Graph;

public class EntraGroup
{
    [Key]
    public long RowId { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string? Id { get; set; }
    public string? Description { get; set; }
    public string? DisplayName { get; set; }
    public string? SecurityIdentifier { get; set; }
    public string? UniqueName { get; set; }
}
