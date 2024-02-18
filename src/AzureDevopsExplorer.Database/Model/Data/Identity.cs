using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

// Microsoft.VisualStudio.Services.Identity.Identity
[PrimaryKey(nameof(Id))]
public class Identity
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public Guid? NamespaceId { get; set; }
    public int? RequiredPermissions { get; set; }
    public string? DescriptorIdentityType { get; set; }
    public string? DescriptorIdentifier { get; set; }
    public string? SubjectDescriptorSubjectType { get; set; }
    public string? SubjectDescriptorIdentifier { get; set; }
    public string? SocialDescriptorSocialType { get; set; }
    public string? SocialDescriptorIdentifier { get; set; }
    public string? ProviderDisplayName { get; set; }
    public string? CustomDisplayName { get; set; }
    public string? DisplayName { get; set; }
    public bool? IsActive { get; set; }
    public int? UniqueUserId { get; set; }
    public bool? IsContainer { get; set; }
    public Guid? MasterId { get; set; }
    //public List<IdentityProperty>? Properties { get; set; }
    public bool? ValidateProperties { get; set; }
    public bool? IsExternalUser { get; set; }
    public Guid? LocalScopeId { get; set; }
    public bool? IsBindPending { get; set; }
    public bool? IsClaims { get; set; }
    public bool? IsImported { get; set; }
    public bool? IsServiceIdentity { get; set; }
    public int? ResourceVersion { get; set; }
    public int? MetaTypeId { get; set; }
    public IdentityMetaType? MetaType { get; set; }
    public bool? IsCspPartnerUser { get; set; }
    public bool? HasModifiedProperties { get; set; }
}

//public class IdentityDescriptor
//{
//    public string IdentityType { get; set; }
//    public string Identifier { get; set; }
//    public System.Object Data { get; set; }
//}

//public class SubjectDescriptor
//{
//    public string SubjectType { get; set; }
//    public string Identifier { get; set; }
//}

public enum IdentityMetaType
{
    Member = 0,
    Guest = 1,
    CompanyAdministrator = 2,
    HelpdeskAdministrator = 3,
    ServiceCloudProvider = 4,
    Unknown = 255
}
