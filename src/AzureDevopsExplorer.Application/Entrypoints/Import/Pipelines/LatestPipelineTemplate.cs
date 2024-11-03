using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.Database.Model.Pipelines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Pipelines;
public class LatestPipelineTemplate
{
    private readonly ILogger logger;
    private readonly AzureDevopsProjectDataContext dataContext;

    public LatestPipelineTemplate(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.CreateLogger(GetType());
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.PipelineRunTemplateImport)
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
            join p in db.LatestPipelineTemplateImport on d.Id equals p.PipelineId into g
            from p in g.DefaultIfEmpty()
            where p == null && d.ConfigurationType == "yaml"
            select new { id = d.Id, revision = d.Revision, type = d.ConfigurationType };

        foreach (var row in missingInTemplateImportsTable.ToList())
        {
            db.LatestPipelineTemplateImport.Add(new LatestPipelineTemplateImport
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
            var lastMonth = DateTimeOffset.UtcNow.AddMonths(-1);
            var imports = db.LatestPipelineTemplateImport
                .Where(x =>
                    x.LastImport == null
                //|| x.LastImport < lastMonth
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
        using var db = dataContext.DataContextFactory.Create();

        var import = await db.LatestPipelineTemplateImport
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
            import.ImportError = err.ToString();
        });

        db.SaveChanges();
    }
}
