using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Commands;
using AzureDevopsExplorer.Database.Extensions;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class BuildsAllCompletedFromStart
{
    private readonly VssConnection connection;
    private readonly string projectName;

    public BuildsAllCompletedFromStart(VssConnection connection, string projectName)
    {
        this.connection = connection;
        this.projectName = projectName;
    }

    public async Task Run(DataConfig config)
    {
        if (config.BuildsAddFromStart == false)
        {
            return;
        }

        var findNonExistingBuildIds = new FindNonExistingBuildIds();
        var findBuildsToAdd = new FindBuildsAndEntities(connection, projectName);

        var carryOn = true;
        var lastBuildIdsRetrieved = "";
        var lastQueueTime = GetBuildsImportState(projectName);

        DateTime? queueTimeBeforeFirstNonCompletedBuild = lastQueueTime;
        if (config.PipelineIds.Count != 0)
        {
            // only write to table if pipelines not specified
            queueTimeBeforeFirstNonCompletedBuild = null;
        }

        while (carryOn)
        {
            var buildsFromApi = await findBuildsToAdd.GetBuildsFromQueueTime(lastQueueTime, config.PipelineIds);

            var buildIds = buildsFromApi.Select(x => x.Id).ToList();
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
                if (build.Status != Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Completed)
                {
                    queueTimeBeforeFirstNonCompletedBuild = null;
                    continue;
                }

                lastQueueTime = build.QueueTime!.Value;
                if (buildIdsToAdd.Contains(build.Id) == false)
                {
                    continue;
                }

                using (var db = new DataContext())
                {
                    db.AddBuild(build);
                    db.SaveChanges();

                    if (queueTimeBeforeFirstNonCompletedBuild != null)
                    {
                        db.SetBuildsQueueTimeOfLastCompletedBuildBeforeNonCompletedValue(projectName, build.QueueTime!.Value);
                    }
                }
            }

            carryOn = buildsFromApi.Any();
        }
    }

    private static DateTime GetBuildsImportState(string projectName)
    {
        using (var applicationContext = new DataContext())
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
