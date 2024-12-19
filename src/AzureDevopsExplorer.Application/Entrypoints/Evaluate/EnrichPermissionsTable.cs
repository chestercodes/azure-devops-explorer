using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain.AccessControlEvaluation;
using AzureDevopsExplorer.Database.Model.Core;
using AzureDevopsExplorer.Database.Model.Security;

namespace AzureDevopsExplorer.Application.Entrypoints.Evaluate;

public class EnrichPermissionsTable
{
    public EnrichPermissionsTable(ICreateDataContexts dataContextFactory)
    {
        this.dataContextFactory = dataContextFactory;
    }

    private readonly ICreateDataContexts dataContextFactory;

    public async Task Run(ProcessConfig config)
    {
        if (config.EnrichPermissionsTable)
        {
            await RunJob();
        }
    }

    private async Task RunJob()
    {
        using var db = dataContextFactory.Create();

        var accessControlAllow =
            db.AccessControl
                .Join(db.SecurityNamespacePermission, ac => new { ac.NamespaceId, Permission = ac.Allow }, snp => new { snp.NamespaceId, snp.Permission }, (ac, snp) => new { ac, snp })
                .Join(db.SecurityNamespaceAction, combined => new { combined.snp.NamespaceId, Bit = combined.snp.ActionBit }, sna => new { sna.NamespaceId, sna.Bit }, (combined, sna) => new { combined, sna })
                .Join(db.SecurityNamespace, combined => combined.sna.NamespaceId, sn => sn.NamespaceId, (combined, sn) => new { combined.combined.ac, combined.combined.snp, combined.sna, sn });

        var accessControlDeny =
            db.AccessControl
                .Join(db.SecurityNamespacePermission, ac => new { ac.NamespaceId, Permission = ac.Deny }, snp => new { snp.NamespaceId, snp.Permission }, (ac, snp) => new { ac, snp })
                .Join(db.SecurityNamespaceAction, combined => new { combined.snp.NamespaceId, Bit = combined.snp.ActionBit }, sna => new { sna.NamespaceId, sna.Bit }, (combined, sna) => new { combined, sna })
                .Join(db.SecurityNamespace, combined => combined.sna.NamespaceId, sn => sn.NamespaceId, (combined, sn) => new { combined.combined.ac, combined.combined.snp, combined.sna, sn });

        var parsedTokens = new Dictionary<(SecurityNamespaceId, string), SecurityNamespacePermissionResourceInfo?>();
        var resourceInfoCache = new Dictionary<SecurityNamespacePermissionResourceInfo, SecurityNamespacePermissionResourceData>();
        var identityDataCache = new Dictionary<string, SecurityNamespacePermissionIdentityData>();

        var identitiesAndIds = new Dictionary<string, Identity>();
        foreach (var identity in db.Identity)
        {
            identitiesAndIds.Add(identity.Descriptor, identity);
        }

        var projects = new Dictionary<Guid, string>();
        foreach (var project in db.Project)
        {
            projects.Add(project.Id, project.Name);
        }

        SecurityNamespacePermissionIdentityData? GetIdentityData(string descriptor)
        {
            if (identityDataCache.ContainsKey(descriptor))
            {
                return identityDataCache[descriptor];
            }
            else
            {
                if (identitiesAndIds.ContainsKey(descriptor))
                {
                    var identity = identitiesAndIds[descriptor];
                    var memberCount = db.IdentityMemberExpanded.Count(x => x.IdentityId == identity.Id);
                    var identityData = new SecurityNamespacePermissionIdentityData(
                        identity.Id,
                        identity.ProviderDisplayName,
                        identity.CustomDisplayName,
                        identity.IsContainer ?? false,
                        memberCount);
                    identityDataCache[descriptor] = identityData;
                    return identityData;
                }
            }
            return null;
        }

        SecurityNamespacePermissionResourceInfo? GetTokenParseResult(SecurityNamespaceId namespaceId, string token)
        {
            var key = (namespaceId, token);
            if (parsedTokens.ContainsKey(key))
            {
                return parsedTokens[key];
            }
            else
            {
                var tokenParseResult = AccessControlTokenParsers.Parse(namespaceId, token);
                parsedTokens.Add(key, tokenParseResult);
                return tokenParseResult;
            }
        }

        SecurityNamespacePermissionResourceData? GetResourceDataFromTable(SecurityNamespacePermissionResourceInfo resourceInfo)
        {
            if (resourceInfo.Type == Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.Environment)
            {
                var valOrNull = db.PipelineEnvironment.SingleOrDefault(x => x.Id.ToString() == resourceInfo.Id.Value);
                if (valOrNull == null)
                {
                    return null;
                }
                return new SecurityNamespacePermissionResourceData(resourceInfo, valOrNull.Name);
            }

            if (resourceInfo.Type == Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.GitRepository)
            {
                if (resourceInfo.Id == null || resourceInfo.Id.Value == null)
                {
                    return null;
                }
                var valOrNull = db.GitRepository.SingleOrDefault(x => x.Id.ToString().ToLower() == resourceInfo.Id.Value.ToLower());
                if (valOrNull == null)
                {
                    return null;
                }
                return new SecurityNamespacePermissionResourceData(resourceInfo, valOrNull.Name);
            }

            if (resourceInfo.Type == Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.Identity)
            {
                var valOrNull = db.Identity.SingleOrDefault(x => x.Id.ToString() == resourceInfo.Id.Value);
                if (valOrNull == null)
                {
                    return null;
                }
                return new SecurityNamespacePermissionResourceData(resourceInfo, valOrNull.ProviderDisplayName);
            }

            if (resourceInfo.Type == Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.SecureFile)
            {
                var valOrNull = db.SecureFile.SingleOrDefault(x => x.Id.ToString() == resourceInfo.Id.Value);
                if (valOrNull == null)
                {
                    return null;
                }
                return new SecurityNamespacePermissionResourceData(resourceInfo, valOrNull.Name);
            }

            if (resourceInfo.Type == Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.ServiceEndpoint)
            {
                var valOrNull = db.ServiceEndpoint.SingleOrDefault(x => x.Id.ToString() == resourceInfo.Id.Value);
                if (valOrNull == null)
                {
                    return null;
                }
                return new SecurityNamespacePermissionResourceData(resourceInfo, valOrNull.Name);
            }

            if (resourceInfo.Type == Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.VariableGroup)
            {
                var valOrNull = db.VariableGroup.SingleOrDefault(x => x.Id.ToString() == resourceInfo.Id.Value);
                if (valOrNull == null)
                {
                    return null;
                }
                return new SecurityNamespacePermissionResourceData(resourceInfo, valOrNull.Name);
            }

            return null;
        }

        SecurityNamespacePermissionResourceData? GetResourceData(SecurityNamespacePermissionResourceInfo resourceInfo)
        {
            if (resourceInfo == null)
            {
                return null;
            }
            if (resourceInfo.Type == null)
            {
                return null;
            }
            if (resourceInfoCache.ContainsKey(resourceInfo))
            {
                return resourceInfoCache[resourceInfo];
            }
            else
            {
                var resourceData = GetResourceDataFromTable(resourceInfo);
                resourceInfoCache.Add(resourceInfo, resourceData);
                return resourceData;
            }

            return null;
        }

        SecurityNamespacePermissionReport GetReportRow(
            SecurityNamespacePermissionAllowOrDeny allowOrDeny,
            AccessControl accessControl,
            SecurityNamespaceAction sna,
            SecurityNamespace sn)
        {
            var reportRow = new SecurityNamespacePermissionReport
            {
                ActionBit = sna.Bit,
                ActionDisplayName = sna.DisplayName,
                ActionName = sna.Name,
                AllowOrDeny = allowOrDeny,
                IdentityDescriptor = accessControl.Descriptor,
                InheritPermissions = accessControl.InheritPermissions,
                NamespaceId = accessControl.NamespaceId,
                NamespaceName = sn.Name,
                Token = accessControl.Token,
            };

            var identityData = GetIdentityData(accessControl.Descriptor);
            if (identityData != null)
            {
                reportRow.IdentityName = identityData.IdentityName;
                reportRow.IdentityIsGroup = identityData.IsGroup;
                reportRow.IdentityDisplayName = identityData.IdentityDisplayName;
                reportRow.IdentityMemberCount = identityData.NumberMembers;
                reportRow.IdentityId = identityData.AzDoId;
            }

            var tokenParseResult = GetTokenParseResult(new SecurityNamespaceId(accessControl.NamespaceId), accessControl.Token);
            if (tokenParseResult != null)
            {
                var projectId = tokenParseResult.ProjectId?.Value;
                if (projectId != null)
                {
                    reportRow.ProjectId = projectId;
                    if (projectId.HasValue && projects.ContainsKey(projectId.Value))
                    {
                        reportRow.ProjectName = projects[projectId.Value];
                    }
                }

                reportRow.ResourceId = tokenParseResult.Id?.Value;
                reportRow.ResourceType = MapResourceType(tokenParseResult.Type);
                reportRow.PermissionScope = MapPermissionScope(tokenParseResult.ParseResultType);
            }

            var resourceData = GetResourceData(tokenParseResult);
            if (resourceData != null)
            {
                reportRow.ResourceName = resourceData.ResourceName;
            }

            return reportRow;
        }

        db.SecurityNamespacePermissionReport.RemoveRange(db.SecurityNamespacePermissionReport);

        foreach (var item in accessControlAllow)
        {
            var reportRow = GetReportRow(
                SecurityNamespacePermissionAllowOrDeny.Allow,
                item.ac,
                item.sna,
                item.sn);
            db.SecurityNamespacePermissionReport.Add(reportRow);
        }

        foreach (var item in accessControlDeny)
        {
            var reportRow = GetReportRow(
                SecurityNamespacePermissionAllowOrDeny.Deny,
                item.ac,
                item.sna,
                item.sn);
            db.SecurityNamespacePermissionReport.Add(reportRow);
        }

        db.SaveChanges();
    }

    private AccessControlTokenPermissionScope? MapPermissionScope(AccessControlTokenParseResultType parseResultType)
    {
        switch (parseResultType)
        {
            case AccessControlTokenParseResultType.OrganisationLevel:
                return AccessControlTokenPermissionScope.Organisation;
            case AccessControlTokenParseResultType.CollectionObjectLevel:
                return AccessControlTokenPermissionScope.CollectionResource;
            case AccessControlTokenParseResultType.ProjectLevel:
                return AccessControlTokenPermissionScope.Project;
            case AccessControlTokenParseResultType.ProjectObjectLevel:
                return AccessControlTokenPermissionScope.ProjectResource;
            default: return null;
        }
    }

    private Database.Model.Security.SecurityNamespacePermissionResourceType? MapResourceType(Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType? type)
    {
        switch (type)
        {
            case Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.AgentPool:
                return Database.Model.Security.SecurityNamespacePermissionResourceType.AgentPool;
            case Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.Build:
                return Database.Model.Security.SecurityNamespacePermissionResourceType.Build;
            case Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.Environment:
                return Database.Model.Security.SecurityNamespacePermissionResourceType.Environment;
            case Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.GitRepository:
                return Database.Model.Security.SecurityNamespacePermissionResourceType.GitRepository;
            case Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.Identity:
                return Database.Model.Security.SecurityNamespacePermissionResourceType.Identity;
            case Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.Organisation:
                return Database.Model.Security.SecurityNamespacePermissionResourceType.Organisation;
            case Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.Project:
                return Database.Model.Security.SecurityNamespacePermissionResourceType.Project;
            case Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.SecureFile:
                return Database.Model.Security.SecurityNamespacePermissionResourceType.SecureFile;
            case Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.ServiceEndpoint:
                return Database.Model.Security.SecurityNamespacePermissionResourceType.ServiceEndpoint;
            case Domain.AccessControlEvaluation.SecurityNamespacePermissionResourceType.VariableGroup:
                return Database.Model.Security.SecurityNamespacePermissionResourceType.VariableGroup;
            default: return null;
        }
    }
}
