using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database.Extensions;
using AzureDevopsExporter.Application.Domain;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Pipelines;
public class BuildsLatestDefaultFromPipeline
{
    private readonly ILogger logger;
    private readonly AzureDevopsProjectDataContext dataContext;

    public BuildsLatestDefaultFromPipeline(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.Create(this);
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.BuildsAddLatestDefaultFromPipeline == false)
        {
            return;
        }

        logger.LogInformation($"Running latest default pipeline import");

        List<int> pipelineIdsFromDb = new List<int>();
        using (var db = dataContext.DataContextFactory.Create())
        {
            pipelineIdsFromDb = db.PipelineCurrent.Where(x => x.ProjectId == dataContext.Project.ProjectId).Select(x => x.Id).ToList();
        }
        if (config.PipelineIds.Count != 0)
        {
            pipelineIdsFromDb = pipelineIdsFromDb.Where(x => config.PipelineIds.Contains(x)).ToList();
        }

        foreach (var pipelineId in pipelineIdsFromDb)
        {
            await AddForPipelineRef(pipelineId);
        }
    }

    private async Task AddForPipelineRef(int pipelineId)
    {
        logger.LogDebug("Running latest default pipeline import for {PipelineId}", pipelineId);

        var buildsResult = await dataContext.Queries.Build.GetPipelineBuildsQueueDescending(pipelineId);
        if (buildsResult.IsT1)
        {
            logger.LogError(buildsResult.AsT1.AsError);
            return;
        }

        var builds = buildsResult.AsT0.value;
        var latestBuildFinder = new LatestBuildFinder();
        var build = latestBuildFinder.GetLatestDefaultBuild(builds.ToList());
        if (build == null)
        {
            logger.LogInformation($"Could not get build ids for {pipelineId}");
            return;
        }

        using var db = dataContext.DataContextFactory.Create();
        if (db.Build.Any(x => x.Id == build.id) == false)
        {
            db.AddBuild(build);
            db.SaveChanges();
        }
    }
}
