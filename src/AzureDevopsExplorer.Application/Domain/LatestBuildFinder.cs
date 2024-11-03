using AzureDevopsExplorer.AzureDevopsApi.Build;

namespace AzureDevopsExporter.Application.Domain;

public class LatestBuildFinder
{
    public BuildDto GetLatestDefaultBuild(List<BuildDto> builds)
    {
        foreach (var build in builds)
        {
            if (build.status != BuildStatus.Completed)
            {
                continue;
            }

            if (build.sourceBranch == "refs/heads/master" || build.sourceBranch == "refs/heads/main")
            {
                return build;
            }
        }

        return null;
    }
}