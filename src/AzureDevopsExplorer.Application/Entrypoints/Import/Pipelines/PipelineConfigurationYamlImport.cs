using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.Database.Model.Pipelines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Pipelines;
public class PipelineConfigurationYamlImport
{
    private readonly ILogger logger;
    private readonly AzureDevopsProjectDataContext dataContext;

    public PipelineConfigurationYamlImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.Create(this);
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.PipelineConfigurationYamlTemplateImport)
        {
            await RunAddMissingPipelines();
            await RunForOlderAndMissing();
        }
    }

    public async Task RunAddMissingPipelines()
    {
        using var db = dataContext.DataContextFactory.Create();

        var missingInTemplateImportsTable =
            from d in db.Pipeline
            join p in db.PipelineConfigurationYamlTemplateImport on d.Id equals p.PipelineId into g
            from p in g.DefaultIfEmpty()
            where p == null && d.ConfigurationType == "yaml"
            select new { id = d.Id, revision = d.Revision, type = d.ConfigurationType };

        foreach (var row in missingInTemplateImportsTable.ToList())
        {
            db.PipelineConfigurationYamlTemplateImport.Add(new PipelineConfigurationYamlTemplateImport
            {
                PipelineId = row.id,
                PipelineRevision = row.revision,
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task RunForOlderAndMissing()
    {
        List<(int PipelineId, int PipelineRevision)> pipelineImports = new();

        using (var db = dataContext.DataContextFactory.Create())
        {
            var lastMonth = DateTime.UtcNow.AddMonths(-1);
            var imports = db.PipelineConfigurationYamlTemplateImport
                .Where(x => 
                (x.LastImport == null || x.LastImport < lastMonth)
                    &&
                (x.ImportError == null)
                    &&
                    db.PipelineCurrent
                        .Where(y => y.ProjectId == dataContext.Project.ProjectId)
                        .Select(y => y.Id)
                        .Contains(x.PipelineId)
                )
                .ToList()
                .Select(x => new { pid = x.PipelineId, pr = x.PipelineRevision })
                .ToList();
            pipelineImports = imports.Select(x => (x.pid, x.pr)).ToList();
        }

        var lastImportTime = DateTime.UtcNow;
        foreach (var pipeline in pipelineImports)
        {
            await RunForPipeline(pipeline, lastImportTime);
        }
    }

    private async Task RunForPipeline((int PipelineId, int PipelineRevision) pipeline, DateTime lastImportTime)
    {
        logger.LogDebug($"Running pipeline config import " + pipeline.PipelineId);

        using var db = dataContext.DataContextFactory.Create();

        var import = await db.PipelineConfigurationYamlTemplateImport
            .Where(x => x.PipelineId == pipeline.PipelineId && x.PipelineRevision == pipeline.PipelineRevision)
            .SingleAsync();

        var pipelineYaml = await db.Pipeline
            .Where(x => x.Id == pipeline.PipelineId && x.Revision == pipeline.PipelineRevision)
            .SingleAsync();

        import.LastImport = lastImportTime;

        var fileResult = await dataContext.Queries.Git.GetFile(pipelineYaml.ConfigurationRepositoryId.Value.ToString(), pipelineYaml.ConfigurationPath);
        fileResult.Switch(fileContent =>
        {
            var parser = new ParseYamlFile();
            var resForExtends = parser.ParsePipelineYamlForExtends(fileContent);
            resForExtends.Switch(parserResult =>
            {
                if (parserResult.TemplateExtendsFromRepository)
                {
                    import.TemplateExtendsPath = parserResult.Info.Path;
                    import.TemplateExtendsRepository = parserResult.Info.Repository;
                    import.TemplateExtendsRef = parserResult.Info.Branch;
                }
            },
            err =>
            {
                import.ImportError = err.ToString();
            });

            var resForSchedules = parser.ParsePipelineYamlForSchedules(fileContent);
            resForSchedules.Switch(schedulesResult =>
            {
                if (schedulesResult.TemplateIsOnSchedule)
                {
                    import.TemplateSchedules = schedulesResult.Data;
                }
            },
            err =>
            {
                import.ImportError = err.ToString();
            });
        },
        err =>
        {
            import.ImportError = err.AsError;
        });

        db.SaveChanges();
    }
}
