﻿using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database;

namespace AzureDevopsExplorer.Application.Entrypoints.Evaluate;

public class RunEvaluate
{
    private readonly ICreateDataContexts dataContext;
    private readonly CancellationToken cancellationToken;

    public RunEvaluate(ICreateDataContexts dataContext, CancellationToken cancellationToken)
    {
        this.dataContext = dataContext;
        this.cancellationToken = cancellationToken;
    }
    public async Task Run(ProcessConfig processConfig)
    {
        if (processConfig.UpdateLatestPipelineAndRun)
        {
            var latestPipelineAndRun = new LatestPipelineAndRun(dataContext);
            await latestPipelineAndRun.Run();
        }

        if (processConfig.DerivePermissions)
        {
            var deriveResourcePermissions = new DeriveResourcePermissions();
            await deriveResourcePermissions.Run();
        }
    }
}
