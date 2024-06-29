using AzureDevopsExplorer.Application.Configuration;

namespace AzureDevopsExplorer.Application.Entrypoints.Evaluate;

public class RunEvaluate
{
    public async Task Run(ProcessConfig processConfig)
    {
        if (processConfig.UpdateLatestPipelineAndRun)
        {
            var latestPipelineAndRun = new LatestPipelineAndRun();
            await latestPipelineAndRun.Run();
        }

        if (processConfig.DerivePermissions)
        {
            var deriveResourcePermissions = new DeriveResourcePermissions();
            await deriveResourcePermissions.Run();
        }
    }
}
