using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

public class IdentityProperty
{
    public int Id { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid IdentityId { get; set; }
    public string Key { get; set; }
    public string? Value { get; set; }
}
