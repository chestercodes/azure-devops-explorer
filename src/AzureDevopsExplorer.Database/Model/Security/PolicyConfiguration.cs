using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Security;

[PrimaryKey(nameof(Id))]
public class PolicyConfiguration
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public DateTime? LastImport { get; set; }


    public Guid ProjectId { get; set; }
    public Guid? CreatedById { get; set; }
    public string? CreatedByDescriptor { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsBlocking { get; set; }
    public bool IsDeleted { get; set; }
    public List<PolicyConfigurationSetting> Settings { get; set; } = new();
    public bool? IsEnterpriseManaged { get; set; }
    public int? Revision { get; set; }

    public string? TypeId { get; set; }
    public string? TypeUrl { get; set; }
    public string? TypeDisplayName { get; set; }
}


public class PolicyConfigurationSetting
{
    public long Id { get; set; }
    public int PolicyConfigurationId { get; set; }
    public string Name { get; set; }
    public string? Value { get; set; }
}