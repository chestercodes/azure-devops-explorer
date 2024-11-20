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
            var lastWeek = DateTime.UtcNow.AddDays(-7);
            var imports = db.PipelineConfigurationYamlTemplateImport
                .Where(x =>
                (
                    // is newly added
                    x.LastImport == null
                    ||
                    // has been previously successfully added and not imported for a month
                    (x.LastImport != null && x.LastImport < lastMonth)
                    ||
                    // has been previously unsuccessfully added and not retried for a week
                    (x.ImportError != null && x.LastImport < lastWeek)
                )
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

    private async Task RunForPipeline((int PipelineId, int PipelineRevision) pipelineRef, DateTime lastImportTime)
    {
        logger.LogDebug($"Running pipeline config import " + pipelineRef.PipelineId);

        using var db = dataContext.DataContextFactory.Create();

        var import = await db.PipelineConfigurationYamlTemplateImport
            .Where(x => x.PipelineId == pipelineRef.PipelineId && x.PipelineRevision == pipelineRef.PipelineRevision)
            .SingleAsync();

        var pipelineConfig = await db.PipelineConfigurationYamlTemplate
            .Where(x => x.PipelineId == pipelineRef.PipelineId && x.PipelineRevision == pipelineRef.PipelineRevision)
            .SingleOrDefaultAsync();

        if (pipelineConfig == null)
        {
            pipelineConfig = new PipelineConfigurationYamlTemplate
            {
                PipelineId = pipelineRef.PipelineId,
                PipelineRevision = pipelineRef.PipelineRevision,
            };
            db.PipelineConfigurationYamlTemplate.Add(pipelineConfig);
        }

        var pipeline = await db.Pipeline
            .Where(x => x.Id == pipelineRef.PipelineId && x.Revision == pipelineRef.PipelineRevision)
            .SingleAsync();

        import.LastImport = lastImportTime;

        var fileResult = await dataContext.Queries.Git.GetFile(pipeline.ConfigurationRepositoryId.Value.ToString(), pipeline.ConfigurationPath);
        fileResult.Switch(fileContent =>
        {
            var parser = new ParseYamlFile();
            var resForExtends = parser.ParsePipelineYamlForExtends(fileContent);
            resForExtends.Switch(parserResult =>
            {
                if (parserResult.TemplateExtendsFromRepository)
                {
                    pipelineConfig.TemplateExtendsPath = parserResult.Info.Path;
                    pipelineConfig.TemplateExtendsRepository = parserResult.Info.Repository;
                    pipelineConfig.TemplateExtendsRef = parserResult.Info.Branch;
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
                    pipelineConfig.TemplateSchedules = schedulesResult.Data;
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
