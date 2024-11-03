using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database.Extensions;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Pipelines;
public class BuildEntitiesImport
{
    private readonly ILogger logger;
    private readonly AzureDevopsProjectDataContext dataContext;

    public BuildEntitiesImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.CreateLogger(GetType());
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
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
        using var db = dataContext.DataContextFactory.Create();

        var missingInImportsTable =
            from b in db.Build
            join i in db.BuildImport on b.Id equals i.BuildRunId into g
            from p in g.DefaultIfEmpty()
            where p == null
            select new { id = b.Id, pipelineId = b.DefinitionId, pipelineRevision = b.DefinitionRevision };

        foreach (var row in missingInImportsTable.ToList())
        {
            db.BuildImport.Add(new Database.Model.Pipelines.BuildImport
            {
                BuildRunId = row.id,
                PipelineId = row.pipelineId,
                PipelineRevision = row.pipelineRevision,
                ArtifactImportErrorHash = null,
                ArtifactImportState = Database.Model.Pipelines.BuildImportState.Initial,
                PipelineRunImportErrorHash = null,
                PipelineRunImportState = Database.Model.Pipelines.BuildImportState.Initial,
                TimelineImportErrorHash = null,
                TimelineImportState = Database.Model.Pipelines.BuildImportState.Initial
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task RunAddBuildArtifacts()
    {
        List<int> buildIds = new();
        using (var db = dataContext.DataContextFactory.Create())
        {
            var buildsToImport =
                db.BuildImport
                    .Where(x => x.ArtifactImportState == Database.Model.Pipelines.BuildImportState.Initial)
                    .Join(
                        db.PipelineCurrent.Where(x => x.ProjectId == dataContext.Project.ProjectId),
                        b => b.PipelineId,
                        p => p.Id,
                        (b, p) => b.BuildRunId);
            buildIds = buildsToImport.ToList();
        }

        foreach (var buildId in buildIds)
        {
            await AddBuildArtifacts(buildId);
        }
    }

    private async Task AddBuildArtifacts(int buildId)
    {
        using var db = dataContext.DataContextFactory.Create();
        var build = db.BuildImport.Where(x => x.BuildRunId == buildId).Single();
        var buildArtifactsResult = await dataContext.Queries.Build.GetArtifacts(build.BuildRunId);
        if (buildArtifactsResult.IsT1)
        {
            build.ArtifactImportState = Database.Model.Pipelines.BuildImportState.ErrorFromApi;
            var errorHash = db.AddImportError(buildArtifactsResult.AsT1.AsError);
            build.ArtifactImportErrorHash = errorHash;
            await db.SaveChangesAsync();
            return;
        }
        var buildArtifacts = buildArtifactsResult.AsT0.value;
        db.AddBuildArtifacts(buildArtifacts, buildId);
        build.ArtifactImportState = Database.Model.Pipelines.BuildImportState.Done;
        await db.SaveChangesAsync();
    }

    public async Task RunAddBuildTimeline()
    {
        List<int> buildIds = new();
        using (var db = dataContext.DataContextFactory.Create())
        {
            var buildsToImport =
                db.BuildImport
                    .Where(x => x.TimelineImportState == Database.Model.Pipelines.BuildImportState.Initial)
                    .Join(
                        db.PipelineCurrent.Where(x => x.ProjectId == dataContext.Project.ProjectId),
                        b => b.PipelineId,
                        p => p.Id,
                        (b, p) => b.BuildRunId);
            buildIds = buildsToImport.ToList();
        }

        foreach (var buildId in buildIds)
        {
            await AddTimeline(buildId);
        }
    }

    private async Task AddTimeline(int buildId)
    {
        using var db = dataContext.DataContextFactory.Create();
        var build = db.BuildImport.Where(x => x.BuildRunId == buildId).Single();
        var buildTimelineResult = await dataContext.Queries.Build.GetTimeline(buildId);
        if (buildTimelineResult.IsT1)
        {
            build.TimelineImportState = Database.Model.Pipelines.BuildImportState.ErrorFromApi;
            var errorHash = db.AddImportError(buildTimelineResult.AsT1.AsError);
            build.TimelineImportErrorHash = errorHash;
            await db.SaveChangesAsync();
            return;
        }
        var buildTimeline = buildTimelineResult.AsT0;
        if (buildTimeline != null)
        {
            db.AddBuildTimeline(buildTimeline, buildId);
        }

        build.TimelineImportState = Database.Model.Pipelines.BuildImportState.Done;
        await db.SaveChangesAsync();
    }

    public async Task RunAddPipelineRun()
    {
        List<(int PipelineId, int BuildId)> pipelineAndBuildIds = new();
        using (var db = dataContext.DataContextFactory.Create())
        {
            var buildsToImport =
                db.BuildImport
                    .Where(x => x.PipelineRunImportState == Database.Model.Pipelines.BuildImportState.Initial)
                    .Join(
                        db.PipelineCurrent.Where(x => x.ProjectId == dataContext.Project.ProjectId),
                        b => b.PipelineId,
                        p => p.Id,
                        (b, p) => new { PipelineId = p.Id, BuildId = b.BuildRunId })
                    .ToList();
            pipelineAndBuildIds = buildsToImport.Select(x => (x.PipelineId, x.BuildId)).ToList();
        }

        foreach (var ids in pipelineAndBuildIds)
        {
            await AddPipelineRun(ids);
        }
    }

    private async Task AddPipelineRun((int PipelineId, int BuildId) ids)
    {
        using var db = dataContext.DataContextFactory.Create();
        var build = db.BuildImport.Where(x => x.BuildRunId == ids.BuildId).Single();
        var pipelineRunResult = await dataContext.Queries.Pipelines.GetPipelineRun(ids.PipelineId, ids.BuildId);
        pipelineRunResult.Switch(pipelineRun =>
        {
            if (pipelineRun == null)
            {
                build.PipelineRunImportState = Database.Model.Pipelines.BuildImportState.ErrorFromApi;
                var errorHash = db.AddImportError("Pipeline run is null!!!");
                build.PipelineRunImportErrorHash = errorHash;
            }
            else
            {
                db.AddPipelineRun(pipelineRun);
                build.PipelineRunImportState = Database.Model.Pipelines.BuildImportState.Done;
            }
            db.SaveChanges();
        }, err =>
        {
            build.PipelineRunImportState = Database.Model.Pipelines.BuildImportState.ErrorFromApi;
            var errorHash = db.AddImportError(err.AsError);
            build.PipelineRunImportErrorHash = errorHash;
            db.SaveChanges();
        });
    }
}
