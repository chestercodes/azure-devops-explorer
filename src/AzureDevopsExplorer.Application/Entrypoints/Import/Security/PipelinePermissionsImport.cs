using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.AzureDevopsApi.ApprovalsAndChecks;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Security;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Security;
public class PipelinePermissionsImport
{
    private readonly ILogger logger;
    private readonly Mappers mapper;
    private readonly Lazy<AzureDevopsApiResult<List<PipelineId>>> projectPipelinesResult;
    private readonly AzureDevopsProjectDataContext dataContext;
    private readonly Guid projectId;

    public PipelinePermissionsImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.CreateLogger(GetType());
        mapper = new Mappers();
        this.dataContext = dataContext;
        projectId = dataContext.Project.ProjectId;

        projectPipelinesResult = new Lazy<AzureDevopsApiResult<List<PipelineId>>>(() =>
        {
            using var db = dataContext.DataContextFactory.Create();
            var pipelineIds = db.PipelineCurrent.Where(x => x.ProjectId == dataContext.Project.ProjectId).Select(x => x.Id).ToList();
            return pipelineIds.Select(x => new PipelineId(x)).ToList();
        });
    }

    public async Task Run(ImportConfig config)
    {
        if (config.PipelinePermissionsImport)
        {
            await AddEnvironmentsPipelineApprovalPermissions();
            await AddServiceConnectionsPipelineApprovalPermissions();
            await AddVariableGroupsPipelineApprovalPermissions();
        }
    }

    public async Task AddEnvironmentsPipelineApprovalPermissions()
    {
        var importTime = DateTime.UtcNow;

        var environmentIds = new List<int>();
        using (var db = dataContext.DataContextFactory.Create())
        {
            environmentIds = db.PipelineEnvironment
                .Where(x => x.ProjectId == projectId)
                .Select(x => x.Id)
                .ToList();
        }

        foreach (var environmentId in environmentIds)
        {
            await RunForEnvironment(environmentId, importTime);
        }
    }

    private async Task RunForEnvironment(int environmentId, DateTime importTime)
    {
        using var db = dataContext.DataContextFactory.Create();
        var existingPermissions = db.PipelineEnvironmentPipelinePermission
            .Where(x => x.PipelineEnvironmentId == environmentId && x.ProjectId == projectId)
            .ToList();

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        if (existingPermissions.Any())
        {
            if (existingPermissions.First().LastImport > sevenDaysAgo)
            {
                // last import was in the last week, don't bother
                return;
            }
        }

        Func<Task<AzureDevopsApiResult<PipelineResourceApproval>>> getPipelinePermissions = () => dataContext.Queries.ApprovalsAndChecks.GetPipelineApprovedEnvironments(environmentId);
        Func<PipelineResourceApprovalAllPipelines, PipelineId, PipelineEnvironmentPipelinePermission> mapAllPipeline = (all, pipelineId) =>
        {
            return new PipelineEnvironmentPipelinePermission
            {
                Authorised = all.authorized,
                AuthorisedById = Guid.Parse(all.authorizedBy.id),
                AuthorisedOn = all.authorizedOn,
                ProjectId = projectId,
                LastImport = importTime,
                PipelineId = pipelineId.Value,
                PipelineEnvironmentId = environmentId
            };
        };
        Func<PipelineResourceApprovalPipeline, PipelineEnvironmentPipelinePermission> mapPipeline = (approval) =>
        {
            return new PipelineEnvironmentPipelinePermission
            {
                Authorised = approval.authorized,
                AuthorisedById = Guid.Parse(approval.authorizedBy.id),
                AuthorisedOn = approval.authorizedOn,
                ProjectId = projectId,
                LastImport = importTime,
                PipelineId = approval.id,
                PipelineEnvironmentId = environmentId
            };
        };

        var permissionsFromApi = await GetPermissionsFromApi(getPipelinePermissions, mapAllPipeline, mapPipeline);
        if (permissionsFromApi == null)
        {
            // errored and has been to console
            return;
        }

        // todo, track changes? probably not useful as likely append only
        db.PipelineEnvironmentPipelinePermission.RemoveRange(existingPermissions);
        db.PipelineEnvironmentPipelinePermission.AddRange(permissionsFromApi);
        await db.SaveChangesAsync();
    }


    public async Task AddServiceConnectionsPipelineApprovalPermissions()
    {
        var importTime = DateTime.UtcNow;

        var serviceEndpointsIds = new List<Guid>();
        using (var db = dataContext.DataContextFactory.Create())
        {
            serviceEndpointsIds = db.ServiceEndpoint
                .Join(db.ServiceEndpointProjectReference,
                    se => se.Id,
                    sepr => sepr.ServiceEndpointId,
                    (se, sepr) => new { se.Id, sepr.ProjectReferenceId }
                )
                .Where(x => x.ProjectReferenceId == projectId)
                .Select(x => x.Id)
                .ToList();
        }

        foreach (var serviceEndpointId in serviceEndpointsIds)
        {
            await RunForServiceEndpoint(serviceEndpointId, importTime);
        }
    }

    private async Task RunForServiceEndpoint(Guid serviceEndpointId, DateTime importTime)
    {
        using var db = dataContext.DataContextFactory.Create();
        var existingPermissions = db.ServiceEndpointPipelinePermission
            .Where(x => x.ServiceEndpointId == serviceEndpointId && x.ProjectId == projectId)
            .ToList();

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        if (existingPermissions.Any())
        {
            if (existingPermissions.First().LastImport > sevenDaysAgo)
            {
                // last import was in the last week, don't bother
                return;
            }
        }

        Func<Task<AzureDevopsApiResult<PipelineResourceApproval>>> getPipelinePermissions = () => dataContext.Queries.ApprovalsAndChecks.GetPipelineApprovedServiceEndpoints(serviceEndpointId);
        Func<PipelineResourceApprovalAllPipelines, PipelineId, ServiceEndpointPipelinePermission> mapAllPipeline = (all, pipelineId) =>
        {
            return new ServiceEndpointPipelinePermission
            {
                Authorised = all.authorized,
                AuthorisedById = Guid.Parse(all.authorizedBy.id),
                AuthorisedOn = all.authorizedOn,
                ProjectId = projectId,
                LastImport = importTime,
                PipelineId = pipelineId.Value,
                ServiceEndpointId = serviceEndpointId
            };
        };
        Func<PipelineResourceApprovalPipeline, ServiceEndpointPipelinePermission> mapPipeline = (approval) =>
        {
            return new ServiceEndpointPipelinePermission
            {
                Authorised = approval.authorized,
                AuthorisedById = Guid.Parse(approval.authorizedBy.id),
                AuthorisedOn = approval.authorizedOn,
                ProjectId = projectId,
                LastImport = importTime,
                PipelineId = approval.id,
                ServiceEndpointId = serviceEndpointId
            };
        };

        var permissionsFromApi = await GetPermissionsFromApi(getPipelinePermissions, mapAllPipeline, mapPipeline);
        if (permissionsFromApi == null)
        {
            // errored and has been to console
            return;
        }

        // todo, track changes? probably not useful as likely append only
        db.ServiceEndpointPipelinePermission.RemoveRange(existingPermissions);
        db.ServiceEndpointPipelinePermission.AddRange(permissionsFromApi);
        await db.SaveChangesAsync();
    }

    public async Task AddVariableGroupsPipelineApprovalPermissions()
    {
        var importTime = DateTime.UtcNow;

        var variableGroupIds = new List<int>();
        using (var db = dataContext.DataContextFactory.Create())
        {
            variableGroupIds = db.VariableGroup
                .Join(db.VariableGroupProjectReference,
                    vg => vg.Id,
                    vgpr => vgpr.Id,
                    (vg, vgpr) => new { vg.Id, vgpr.ProjectReferenceId }
                )
                .Where(x => x.ProjectReferenceId == projectId)
                .Select(x => x.Id)
                .ToList();
        }

        foreach (var variableGroup in variableGroupIds)
        {
            await RunForVariableGroup(variableGroup, importTime);
        }
    }

    private async Task RunForVariableGroup(int variableGroupId, DateTime importTime)
    {
        using var db = dataContext.DataContextFactory.Create();
        var existingPermissions = db.VariableGroupPipelinePermission
            .Where(x => x.VariableGroupId == variableGroupId && x.ProjectId == projectId)
            .ToList();

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        if (existingPermissions.Any())
        {
            if (existingPermissions.First().LastImport > sevenDaysAgo)
            {
                // last import was in the last week, don't bother
                return;
            }
        }

        Func<Task<AzureDevopsApiResult<PipelineResourceApproval>>> getPipelinePermissions = () => dataContext.Queries.ApprovalsAndChecks.GetPipelineApprovedVariableGroup(variableGroupId);
        Func<PipelineResourceApprovalAllPipelines, PipelineId, VariableGroupPipelinePermission> mapAllPipeline = (all, pipelineId) =>
        {
            return new VariableGroupPipelinePermission
            {
                Authorised = all.authorized,
                AuthorisedById = Guid.Parse(all.authorizedBy.id),
                AuthorisedOn = all.authorizedOn,
                ProjectId = projectId,
                LastImport = importTime,
                PipelineId = pipelineId.Value,
                VariableGroupId = variableGroupId
            };
        };
        Func<PipelineResourceApprovalPipeline, VariableGroupPipelinePermission> mapPipeline = (approval) =>
        {
            return new VariableGroupPipelinePermission
            {
                Authorised = approval.authorized,
                AuthorisedById = Guid.Parse(approval.authorizedBy.id),
                AuthorisedOn = approval.authorizedOn,
                ProjectId = projectId,
                LastImport = importTime,
                PipelineId = approval.id,
                VariableGroupId = variableGroupId
            };
        };

        var permissionsFromApi = await GetPermissionsFromApi(getPipelinePermissions, mapAllPipeline, mapPipeline);
        if (permissionsFromApi == null)
        {
            // errored and has been to console
            return;
        }

        // todo, track changes? probably not useful as likely append only
        db.VariableGroupPipelinePermission.RemoveRange(existingPermissions);
        db.VariableGroupPipelinePermission.AddRange(permissionsFromApi);
        await db.SaveChangesAsync();
    }

    private async Task<List<TDb>?> GetPermissionsFromApi<TDb>(
        Func<Task<AzureDevopsApiResult<PipelineResourceApproval>>> getPipelinePermissions,
        Func<PipelineResourceApprovalAllPipelines, PipelineId, TDb> mapAllPipeline,
        Func<PipelineResourceApprovalPipeline, TDb> mapPipeline
        )
    {
        List<TDb> permissionsFromApi = new();

        var pipelinePermissions = await getPipelinePermissions();
        if (pipelinePermissions.IsT1)
        {
            logger.LogError(pipelinePermissions.AsT1.AsError);
            return null;
        }

        var permissions = pipelinePermissions.AsT0;
        if (permissions.allPipelines != null)
        {
            var allPipelinesResult = projectPipelinesResult.Value;
            if (allPipelinesResult.IsT1)
            {
                logger.LogError(allPipelinesResult.AsT1.AsError);
                return null;
            }

            permissionsFromApi = allPipelinesResult.AsT0
                .Select(x => mapAllPipeline(permissions.allPipelines, x))
                .ToList();
        }
        else
        {
            if (permissions.pipelines != null)
            {
                permissionsFromApi = permissions.pipelines.Select(mapPipeline).ToList();
            }
        }

        return permissionsFromApi;
    }

}
