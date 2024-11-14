using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database.Mappers;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Pipelines;

public class CurrentPipelinesImport
{
    private readonly Mappers mapper;
    private readonly ILogger logger;
    private readonly AzureDevopsProjectDataContext dataContext;

    public CurrentPipelinesImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.Create(this);
        mapper = new Mappers();
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.PipelineCurrentImport)
        {
            await AddPipelines();
        }
    }

    public async Task AddPipelines()
    {
        logger.LogInformation($"Running current pipelines import");

        var importTime = DateTime.UtcNow;
        var pipelineRefsResult = await dataContext.Queries.Pipelines.GetPipelines();
        if (pipelineRefsResult.IsT1)
        {
            logger.LogError(pipelineRefsResult.AsT1.AsError);
            return;
        }

        var pipelinesFromApi = pipelineRefsResult.AsT0
            .Select(x =>
            {
                var mapped = mapper.MapPipelineRef(x);
                mapped.ProjectId = dataContext.Project.ProjectId;
                return mapped;
            })
            .ToList();

        using (var db = dataContext.DataContextFactory.Create())
        {
            db.PipelineCurrent.RemoveRange(db.PipelineCurrent);
            db.PipelineCurrent.AddRange(pipelinesFromApi);
            await db.SaveChangesAsync();
        }
    }
}
