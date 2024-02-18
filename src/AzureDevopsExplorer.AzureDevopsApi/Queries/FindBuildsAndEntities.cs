namespace AzureDevopsExplorer.AzureDevopsApi;

using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

public class FindBuildsAndEntities
{
    private string projectName;
    private VssConnection connection;

    public FindBuildsAndEntities(VssConnection connection, string projectName)
    {
        this.connection = connection;
        this.projectName = projectName;
    }

    //public async Task<IEnumerable<BuildArtifact>> GetArtifacts(int buildId)
    //{
    //    var buildClient = connection.GetClient<BuildHttpClient>();
    //    var buildArtifacts = await buildClient.GetArtifactsAsync(project: projectName, buildId: buildId);
    //    return buildArtifacts;
    //}

    //public async Task<Timeline> GetTimeline(int buildId)
    //{
    //    var buildClient = connection.GetClient<BuildHttpClient>();
    //    var buildTimeline = await buildClient.GetBuildTimelineAsync(project: projectName, buildId: buildId);
    //    return buildTimeline;
    //}

    public async Task<IEnumerable<Build>> GetBuildsFromQueueTime(DateTime lastImportQueueTime, List<int>? pipelineIds = null)
    {
        var buildClient = connection.GetClient<BuildHttpClient>();
        if (pipelineIds == null || pipelineIds.Count == 0)
        {
            return await buildClient.GetBuildsAsync(project: projectName, minFinishTime: lastImportQueueTime, queryOrder: BuildQueryOrder.QueueTimeAscending);
        }
        return await buildClient.GetBuildsAsync(project: projectName, minFinishTime: lastImportQueueTime, queryOrder: BuildQueryOrder.QueueTimeAscending, definitions: pipelineIds);
    }

    public async Task<IEnumerable<Build>> GetBuildsInProgress()
    {
        var buildClient = connection.GetClient<BuildHttpClient>();
        var builds = await buildClient.GetBuildsAsync(project: projectName, statusFilter: BuildStatus.InProgress, queryOrder: BuildQueryOrder.QueueTimeAscending);
        return builds;
    }
}
