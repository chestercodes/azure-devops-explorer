using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database.Extensions;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Pipelines;
public class PipelineRunImport
{
    private readonly ILogger logger;
    private readonly AzureDevopsProjectDataContext dataContext;

    public PipelineRunImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.Create(this);
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.PipelineRunImport)
        {
            await RunAddMissingBuildsToImport();
            await RunAddPipelineRun();
        }
    }

    public async Task RunAddMissingBuildsToImport()
    {
        using var db = dataContext.DataContextFactory.Create();

        var missingInImportsTable =
            from b in db.Build
            join i in db.PipelineRunImport on b.Id equals i.BuildRunId into g
            from p in g.DefaultIfEmpty()
            where p == null
            select new { id = b.Id, pipelineId = b.DefinitionId, pipelineRevision = b.DefinitionRevision };

        foreach (var row in missingInImportsTable.ToList())
        {
            db.PipelineRunImport.Add(new Database.Model.Pipelines.PipelineRunImport
            {
                BuildRunId = row.id,
                PipelineId = row.pipelineId,
                PipelineRevision = row.pipelineRevision,
                PipelineRunImportErrorHash = null,
                PipelineRunImportState = Database.Model.Pipelines.PipelineRunImportState.Initial
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task RunAddPipelineRun()
    {
        logger.LogInformation($"Running pipeline run import");

        List<(int PipelineId, int BuildId)> pipelineAndBuildIds = new();
        using (var db = dataContext.DataContextFactory.Create())
        {
            var buildsToImport =
                db.PipelineRunImport
                    .Where(x => x.PipelineRunImportState == Database.Model.Pipelines.PipelineRunImportState.Initial)
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
        logger.LogDebug($"Running pipeline run import for {ids.PipelineId} - {ids.BuildId} ");

        using var db = dataContext.DataContextFactory.Create();
        var build = db.PipelineRunImport.Where(x => x.BuildRunId == ids.BuildId).Single();
        var pipelineRunResult = await dataContext.Queries.Pipelines.GetPipelineRun(ids.PipelineId, ids.BuildId);
        pipelineRunResult.Switch(pipelineRun =>
        {
            if (pipelineRun == null)
            {
                build.PipelineRunImportState = Database.Model.Pipelines.PipelineRunImportState.ErrorFromApi;
                var errorHash = db.AddImportError("Pipeline run is null!!!");
                build.PipelineRunImportErrorHash = errorHash;
            }
            else
            {
                db.AddPipelineRun(pipelineRun);
                build.PipelineRunImportState = Database.Model.Pipelines.PipelineRunImportState.Done;
            }
            db.SaveChanges();
        }, err =>
        {
            build.PipelineRunImportState = Database.Model.Pipelines.PipelineRunImportState.ErrorFromApi;
            var errorHash = db.AddImportError(err.AsError);
            build.PipelineRunImportErrorHash = errorHash;
            db.SaveChanges();
        });
    }
}
