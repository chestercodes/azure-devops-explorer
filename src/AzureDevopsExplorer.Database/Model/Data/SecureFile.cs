using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

public class SecureFile
{
    public DateTime? LastImport { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public string? Name { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? CreatedById { get; set; }
    public DateTime? CreatedOn { get; set; }
    public Guid? ModifiedById { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public List<SecureFileProperty> Properties { get; set; } = new();
}

public class SecureFileProperty
{
    public int Id { get; set; }
    public Guid SecureFileId { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
}
