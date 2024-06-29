using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class PipelinePermissionsImport
{
    private readonly ILogger logger;
    private readonly AzureDevopsApiProjectClient httpClient;
    private readonly AzureDevopsApiProjectQueries queries;
    private readonly Mappers mapper;
    private readonly Lazy<AzureDevopsApiResult<List<PipelineRef>>> projectPipelinesResult;

    public PipelinePermissionsImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.GetLogger();
        this.httpClient = dataContext.HttpClient.Value;
        queries = dataContext.Queries.Value;
        mapper = new Mappers();

        projectPipelinesResult = new Lazy<AzureDevopsApiResult<List<PipelineRef>>>(() =>
        {
            var pipelineRefs = new GetPipelineRefs(httpClient);
            return pipelineRefs.GetAll().Result;
        });

    }

    public async Task Run(DataConfig config)
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
        using (var db = new DataContext())
        {
            environmentIds = db.PipelineEnvironment
                .Where(x => x.ProjectId == httpClient.Info.ProjectId)
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
        using var db = new DataContext();
        var existingPermissions = db.PipelineEnvironmentPipelinePermission
            .Where(x => x.PipelineEnvironmentId == environmentId && x.ProjectId == httpClient.Info.ProjectId)
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

        Func<Task<AzureDevopsApiResult<PipelineResourceApproval>>> getPipelinePermissions = () => queries.GetPipelineApprovedEnvironments(environmentId);
        Func<PipelineResourceApprovalAllPipelines, PipelineRef, PipelineEnvironmentPipelinePermission> mapAllPipeline = (all, pipelineRef) =>
        {
            return new PipelineEnvironmentPipelinePermission
            {
                Authorised = all.Authorized,
                AuthorisedById = Guid.Parse(all.AuthorizedBy.Id),
                AuthorisedOn = all.AuthorizedOn,
                ProjectId = httpClient.Info.ProjectId,
                LastImport = importTime,
                PipelineId = pipelineRef.Id,
                PipelineEnvironmentId = environmentId
            };
        };
        Func<PipelineResourceApprovalPipeline, PipelineEnvironmentPipelinePermission> mapPipeline = (approval) =>
        {
            return new PipelineEnvironmentPipelinePermission
            {
                Authorised = approval.Authorized,
                AuthorisedById = Guid.Parse(approval.AuthorizedBy.Id),
                AuthorisedOn = approval.AuthorizedOn,
                ProjectId = httpClient.Info.ProjectId,
                LastImport = importTime,
                PipelineId = approval.Id,
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
        using (var db = new DataContext())
        {
            serviceEndpointsIds = db.ServiceEndpoint
                .Join(db.ServiceEndpointProjectReference,
                    se => se.Id,
                    sepr => sepr.ServiceEndpointId,
                    (se, sepr) => new { se.Id, sepr.ProjectReferenceId }
                )
                .Where(x => x.ProjectReferenceId == httpClient.Info.ProjectId)
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
        using var db = new DataContext();
        var existingPermissions = db.ServiceEndpointPipelinePermission
            .Where(x => x.ServiceEndpointId == serviceEndpointId && x.ProjectId == httpClient.Info.ProjectId)
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

        Func<Task<AzureDevopsApiResult<PipelineResourceApproval>>> getPipelinePermissions = () => queries.GetPipelineApprovedServiceEndpoints(serviceEndpointId);
        Func<PipelineResourceApprovalAllPipelines, PipelineRef, ServiceEndpointPipelinePermission> mapAllPipeline = (all, pipelineRef) =>
        {
            return new ServiceEndpointPipelinePermission
            {
                Authorised = all.Authorized,
                AuthorisedById = Guid.Parse(all.AuthorizedBy.Id),
                AuthorisedOn = all.AuthorizedOn,
                ProjectId = httpClient.Info.ProjectId,
                LastImport = importTime,
                PipelineId = pipelineRef.Id,
                ServiceEndpointId = serviceEndpointId
            };
        };
        Func<PipelineResourceApprovalPipeline, ServiceEndpointPipelinePermission> mapPipeline = (approval) =>
        {
            return new ServiceEndpointPipelinePermission
            {
                Authorised = approval.Authorized,
                AuthorisedById = Guid.Parse(approval.AuthorizedBy.Id),
                AuthorisedOn = approval.AuthorizedOn,
                ProjectId = httpClient.Info.ProjectId,
                LastImport = importTime,
                PipelineId = approval.Id,
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
        using (var db = new DataContext())
        {
            variableGroupIds = db.VariableGroup
                .Join(db.VariableGroupProjectReference,
                    vg => vg.Id,
                    vgpr => vgpr.Id,
                    (vg, vgpr) => new { vg.Id, vgpr.ProjectReferenceId }
                )
                .Where(x => x.ProjectReferenceId == httpClient.Info.ProjectId)
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
        using var db = new DataContext();
        var existingPermissions = db.VariableGroupPipelinePermission
            .Where(x => x.VariableGroupId == variableGroupId && x.ProjectId == httpClient.Info.ProjectId)
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

        Func<Task<AzureDevopsApiResult<PipelineResourceApproval>>> getPipelinePermissions = () => queries.GetPipelineApprovedVariableGroup(variableGroupId);
        Func<PipelineResourceApprovalAllPipelines, PipelineRef, VariableGroupPipelinePermission> mapAllPipeline = (all, pipelineRef) =>
        {
            return new VariableGroupPipelinePermission
            {
                Authorised = all.Authorized,
                AuthorisedById = Guid.Parse(all.AuthorizedBy.Id),
                AuthorisedOn = all.AuthorizedOn,
                ProjectId = httpClient.Info.ProjectId,
                LastImport = importTime,
                PipelineId = pipelineRef.Id,
                VariableGroupId = variableGroupId
            };
        };
        Func<PipelineResourceApprovalPipeline, VariableGroupPipelinePermission> mapPipeline = (approval) =>
        {
            return new VariableGroupPipelinePermission
            {
                Authorised = approval.Authorized,
                AuthorisedById = Guid.Parse(approval.AuthorizedBy.Id),
                AuthorisedOn = approval.AuthorizedOn,
                ProjectId = httpClient.Info.ProjectId,
                LastImport = importTime,
                PipelineId = approval.Id,
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
        Func<PipelineResourceApprovalAllPipelines, PipelineRef, TDb> mapAllPipeline,
        Func<PipelineResourceApprovalPipeline, TDb> mapPipeline
        )
    {
        List<TDb> permissionsFromApi = new();

        var pipelinePermissions = await getPipelinePermissions();
        if (pipelinePermissions.IsT1)
        {
            Console.WriteLine(pipelinePermissions.AsT1.AsError);
            return null;
        }

        var permissions = pipelinePermissions.AsT0;
        if (permissions.AllPipelines != null)
        {
            var allPipelinesResult = projectPipelinesResult.Value;
            if (allPipelinesResult.IsT1)
            {
                Console.WriteLine(allPipelinesResult.AsT1.AsError);
                return null;
            }

            permissionsFromApi = allPipelinesResult.AsT0
                .Select(x => mapAllPipeline(permissions.AllPipelines, x))
                .ToList();
        }
        else
        {
            if (permissions.Pipelines != null)
            {
                permissionsFromApi = permissions.Pipelines.Select(mapPipeline).ToList();
            }
        }

        return permissionsFromApi;
    }

}
