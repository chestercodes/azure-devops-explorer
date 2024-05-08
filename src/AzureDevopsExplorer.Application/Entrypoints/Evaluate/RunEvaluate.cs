namespace AzureDevopsExplorer.Application.Entrypoints.Evaluate;

public class RunEvaluate
{
    public async Task Run()
    {
        var latestPipelineAndRun = new LatestPipelineAndRun();
        await latestPipelineAndRun.Run();

        var deriveResourcePermissions = new DeriveResourcePermissions();
        await deriveResourcePermissions.Run();
    }
}
