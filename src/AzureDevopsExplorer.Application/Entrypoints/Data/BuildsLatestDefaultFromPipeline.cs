using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Extensions;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class BuildsLatestDefaultFromPipeline
{
    private readonly VssConnection vssConnection;
    private readonly AzureDevopsApiProjectClient httpClient;
    private readonly string projectName;

    public BuildsLatestDefaultFromPipeline(VssConnection vssConnection, AzureDevopsApiProjectClient httpClient, string projectName)
    {
        this.vssConnection = vssConnection;
        this.httpClient = httpClient;
        this.projectName = projectName;
    }

    public async Task Run(DataConfig config)
    {
        if (config.BuildsAddLatestDefaultFromPipeline == false)
        {
            return;
        }

        var getAllPipelines = new GetPipelineRefs(httpClient);
        var pipelineRefsResult = await getAllPipelines.GetAll();
        var pipelineRefs = pipelineRefsResult.Match(
            data =>
            {
                if (config.PipelineIds.Count != 0)
                {
                    return data.Where(x => config.PipelineIds.Contains(x.Id));
                }

                return data;
            },
            err =>
            {
                Console.WriteLine(err.AsError);
                throw err.AsT1;
            });

        foreach (var pipelineRef in pipelineRefs)
        {
            await AddForPipelineRef(pipelineRef);
        }
    }

    private async Task AddForPipelineRef(PipelineRef pipelineRef)
    {
        var buildClient = vssConnection.GetClient<BuildHttpClient>();
        var builds = await buildClient.GetBuildsAsync(projectName, definitions: [pipelineRef.Id], queryOrder: BuildQueryOrder.QueueTimeDescending);
        var latestBuildFinder = new LatestBuildFinder();
        var build = latestBuildFinder.GetLatestDefaultBuild(builds);
        if (build == null)
        {
            Console.WriteLine($"Could not get build ids for {pipelineRef.Id} {pipelineRef.Name}");
            return;
        }

        using var db = new DataContext();
        if (db.Build.Any(x => x.Id == build.Id) == false)
        {
            db.AddBuild(build);
            db.SaveChanges();
        }
    }
}
