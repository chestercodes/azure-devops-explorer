using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Graph;

[PrimaryKey(nameof(Id))]
public class EntraApplication
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public Guid AppId { get; set; }
    public string? DisplayName { get; set; }
    public string? UniqueName { get; set; }
    public List<GraphAppRole> AppRoles { get; set; } = new();
}

public class GraphAppRole
{
    public long Id { get; set; }
    public Guid? AppRoleId { get; set; }
    public string? Description { get; set; }
    public string? DisplayName { get; set; }
    public string? Origin { get; set; }
    public bool? IsEnabled { get; set; }
    public string? Value { get; set; }
}
