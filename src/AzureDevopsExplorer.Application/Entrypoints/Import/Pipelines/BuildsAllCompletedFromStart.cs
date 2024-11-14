using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi.Build;
using AzureDevopsExplorer.Database.Commands;
using AzureDevopsExplorer.Database.Extensions;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Pipelines;
public class BuildsAllCompletedFromStart
{
    private readonly ILogger logger;
    private readonly string projectName;
    private readonly AzureDevopsProjectDataContext dataContext;

    public BuildsAllCompletedFromStart(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.Create(this);
        projectName = dataContext.Project.ProjectName;
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.BuildsAddFromStart == false)
        {
            return;
        }

        logger.LogInformation($"Running all builds from start");


        var findNonExistingBuildIds = new FindNonExistingBuildIds(dataContext.DataContextFactory);

        var carryOn = true;
        var lastBuildIdsRetrieved = "";
        var lastQueueTime = GetBuildsImportState(projectName);

        DateTime? queueTimeBeforeFirstNonCompletedBuild = lastQueueTime;
        if (config.PipelineIds.Count != 0)
        {
            logger.LogDebug($"Running for pipelines " + string.Join(", ", config.PipelineIds));

            // only write to table if pipelines not specified
            queueTimeBeforeFirstNonCompletedBuild = null;
        }

        while (carryOn)
        {
            var buildsFromApiResult = await dataContext.Queries.Build.GetBuildsFrom(lastQueueTime, config.PipelineIds);

            if (buildsFromApiResult.IsT1)
            {
                logger.LogError(buildsFromApiResult.AsT1.AsError);
                break;
            }

            var buildsFromApi = buildsFromApiResult.AsT0.value;

            var buildIds = buildsFromApi.Select(x => x.id).ToList();
            var buildIdsList = string.Join(",", buildIds);
            if (buildIdsList == lastBuildIdsRetrieved)
            {
                // this has gotten into a loop, stop running
                carryOn = false;
                continue;
            }
            else
            {
                lastBuildIdsRetrieved = buildIdsList;
            }

            var buildIdsToAdd = findNonExistingBuildIds.Run(buildIds);

            foreach (var build in buildsFromApi)
            {
                if (build.status != BuildStatus.Completed)
                {
                    queueTimeBeforeFirstNonCompletedBuild = null;
                    continue;
                }

                lastQueueTime = build.queueTime;
                if (buildIdsToAdd.Contains(build.id) == false)
                {
                    continue;
                }

                using (var db = dataContext.DataContextFactory.Create())
                {
                    db.AddBuild(build);
                    db.SaveChanges();

                    if (queueTimeBeforeFirstNonCompletedBuild != null)
                    {
                        db.SetBuildsQueueTimeOfLastCompletedBuildBeforeNonCompletedValue(projectName, build.queueTime);
                    }
                }
            }

            carryOn = buildsFromApi.Any();
        }
    }

    private DateTime GetBuildsImportState(string projectName)
    {
        using (var applicationContext = dataContext.DataContextFactory.Create())
        {
            var w = applicationContext.GetBuildsQueueTimeOfLastCompletedBuildBeforeNonCompletedValue(projectName);
            if (w.HasValue)
            {
                return w.Value;
            }
        }

        return DateTime.MinValue;
    }
}
