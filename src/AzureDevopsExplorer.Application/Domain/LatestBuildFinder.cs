
namespace AzureDevopsExporter.Application.Domain;

public class LatestBuildFinder
{
    public Microsoft.TeamFoundation.Build.WebApi.Build GetLatestDefaultBuild(List<Microsoft.TeamFoundation.Build.WebApi.Build> builds)
    {
        foreach (var build in builds)
        {
            if (build.Status != Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Completed)
            {
                continue;
            }

            if (build.SourceBranch == "refs/heads/master" || build.SourceBranch == "refs/heads/main")
            {
                return build;
            }
        }

        return null;
    }
}