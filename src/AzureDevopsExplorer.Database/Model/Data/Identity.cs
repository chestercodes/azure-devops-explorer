using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(Id))]
public class Identity
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public string Descriptor { get; set; }
    public string SubjectDescriptor { get; set; }
    public string? ProviderDisplayName { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsContainer { get; set; }
    public List<IdentityMember> Members { get; set; } = new();
    public List<IdentityMemberOf> MemberOf { get; set; } = new();
    public List<IdentityMemberId> MemberIds { get; set; } = new();
    public List<IdentityProperty> Properties { get; set; } = new();
    public int? ResourceVersion { get; set; }
    public int? MetaTypeId { get; set; }
    public string? CustomDisplayName { get; set; }
    public DateTime LastImport { get; set; }
}

public class IdentityMember
{
    public int Id { get; set; }
    public Guid IdentityId { get; set; }
    public string Descriptor { get; set; }
}

public class IdentityMemberOf
{
    public int Id { get; set; }
    public Guid IdentityId { get; set; }
    public string Descriptor { get; set; }
}

public class IdentityMemberId
{
    public int Id { get; set; }
    public Guid IdentityId { get; set; }
    public Guid MemberId { get; set; }
}

public class IdentityProperty
{
    public int Id { get; set; }
    public Guid IdentityId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
}
