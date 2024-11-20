using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Security;

[PrimaryKey(nameof(Id))]
public class CheckConfiguration
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime? LastImport { get; set; }
    public string? SettingsJson { get; set; }
    public List<CheckConfigurationSetting> Settings { get; set; } = new();
    public Guid? CreatedById { get; set; }
    public DateTime? CreatedOn { get; set; }
    public Guid? ModifiedById { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public int? Timeout { get; set; }
    public int Version { get; set; }
    public string? TypeId { get; set; }
    public string? TypeName { get; set; }
    public string? Url { get; set; }
    public string ResourceType { get; set; }
    public string ResourceId { get; set; }
    public string ResourceName { get; set; }
}

public class CheckConfigurationSetting
{
    public long Id { get; set; }
    public int CheckConfigurationId { get; set; }
    public string Name { get; set; }
    public string? Value { get; set; }
}
