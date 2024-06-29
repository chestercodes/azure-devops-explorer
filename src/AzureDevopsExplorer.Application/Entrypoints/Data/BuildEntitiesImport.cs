using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class BuildEntitiesImport
{
    private readonly ILogger logger;
    private readonly VssConnection vssConnection;
    private readonly AzureDevopsApiProjectClient httpClient;
    private readonly string projectName;

    public BuildEntitiesImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.GetLogger();
        this.vssConnection = dataContext.VssConnection.Value;
        this.httpClient = dataContext.HttpClient.Value;
        this.projectName = dataContext.Project.ProjectName;
    }

    public async Task Run(DataConfig config)
    {
        if (config.BuildAddArtifacts || config.BuildAddTimeline || config.BuildAddPipelineRun)
        {
            await RunAddMissingBuildsToImport();
        }

        if (config.BuildAddArtifacts)
        {
            await RunAddBuildArtifacts();
        }

        if (config.BuildAddTimeline)
        {
            await RunAddBuildTimeline();
        }

        if (config.BuildAddPipelineRun)
        {
            await RunAddPipelineRun();
        }
    }

    public async Task RunAddMissingBuildsToImport()
    {
        using var db = new DataContext();

        var missingInImportsTable =
            from b in db.Build
            join i in db.BuildImport on b.Id equals i.BuildRunId into g
            from p in g.DefaultIfEmpty()
            where p == null
            select new { id = b.Id, pipelineId = b.DefinitionId, pipelineRevision = b.DefinitionRevision };

        foreach (var row in missingInImportsTable.ToList())
        {
            db.BuildImport.Add(new Database.Model.Data.BuildImport
            {
                BuildRunId = row.id,
                PipelineId = row.pipelineId,
                PipelineRevision = row.pipelineRevision,
                ArtifactImportErrorHash = null,
                ArtifactImportState = Database.Model.Data.BuildImportState.Initial,
                PipelineRunImportErrorHash = null,
                PipelineRunImportState = Database.Model.Data.BuildImportState.Initial,
                TimelineImportErrorHash = null,
                TimelineImportState = Database.Model.Data.BuildImportState.Initial
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task RunAddBuildArtifacts()
    {
        var buildClient = vssConnection.GetClient<BuildHttpClient>();
        List<int> buildIds = new();
        using (var db = new DataContext())
        {
            var ids = db.BuildImport.Where(x => x.ArtifactImportState == Database.Model.Data.BuildImportState.Initial)
                .Select(x => x.BuildRunId)
                .ToList();
            buildIds = ids;
        }

        foreach (var buildId in buildIds)
        {
            await AddBuildArtifacts(buildClient, buildId);
        }
    }

    private async Task AddBuildArtifacts(BuildHttpClient buildClient, int buildId)
    {
        using var db = new DataContext();
        var build = db.BuildImport.Where(x => x.BuildRunId == buildId).Single();
        var buildArtifacts = await buildClient.GetArtifactsAsync(projectName, build.BuildRunId);
        db.AddBuildArtifacts(buildArtifacts, buildId);
        build.ArtifactImportState = Database.Model.Data.BuildImportState.Done;
        await db.SaveChangesAsync();
    }

    public async Task RunAddBuildTimeline()
    {
        var buildClient = vssConnection.GetClient<BuildHttpClient>();
        List<int> buildIds = new();
        using (var db = new DataContext())
        {
            var ids = db.BuildImport.Where(x => x.TimelineImportState == Database.Model.Data.BuildImportState.Initial)
                .Select(x => x.BuildRunId)
                .ToList();
            buildIds = ids;
        }

        foreach (var buildId in buildIds)
        {
            await AddTimeline(buildClient, buildId);
        }
    }

    private async Task AddTimeline(BuildHttpClient buildClient, int buildId)
    {
        using var db = new DataContext();
        var build = db.BuildImport.Where(x => x.BuildRunId == buildId).Single();
        var buildTimeline = await buildClient.GetBuildTimelineAsync(projectName, buildId);
        if (buildTimeline != null)
        {
            db.AddBuildTimeline(buildTimeline, buildId);
        }

        build.TimelineImportState = Database.Model.Data.BuildImportState.Done;
        await db.SaveChangesAsync();
    }

    public async Task RunAddPipelineRun()
    {
        List<(int PipelineId, int BuildId)> pipelineAndBuildIds = new();
        using (var db = new DataContext())
        {
            var builds = db.BuildImport
                .Where(x => x.PipelineRunImportState == Database.Model.Data.BuildImportState.Initial)
                .Select(x => new { x.PipelineId, BuildId = x.BuildRunId })
                .ToList();
            pipelineAndBuildIds = builds.Select(x => (x.PipelineId, x.BuildId)).ToList();
        }

        var queries = new AzureDevopsApiProjectQueries(httpClient);

        foreach (var ids in pipelineAndBuildIds)
        {
            await AddPipelineRun(queries, ids);
        }
    }

    private static async Task AddPipelineRun(AzureDevopsApiProjectQueries queries, (int PipelineId, int BuildId) ids)
    {
        using var db = new DataContext();
        var build = db.BuildImport.Where(x => x.BuildRunId == ids.BuildId).Single();
        var pipelineRunResult = await queries.GetPipelineRun(ids.PipelineId, ids.BuildId);
        pipelineRunResult.Switch(pipelineRun =>
        {
            if (pipelineRun == null)
            {
                build.PipelineRunImportState = Database.Model.Data.BuildImportState.ErrorFromApi;
                var errorHash = db.AddImportError("Pipeline run is null!!!");
                build.PipelineRunImportErrorHash = errorHash;
            }
            else
            {
                db.AddPipelineRun(pipelineRun);
                build.PipelineRunImportState = Database.Model.Data.BuildImportState.Done;
            }
            db.SaveChanges();
        }, err =>
        {
            build.PipelineRunImportState = Database.Model.Data.BuildImportState.ErrorFromApi;
            var errorHash = db.AddImportError(err.AsError);
            build.PipelineRunImportErrorHash = errorHash;
            db.SaveChanges();
        });
    }
}
