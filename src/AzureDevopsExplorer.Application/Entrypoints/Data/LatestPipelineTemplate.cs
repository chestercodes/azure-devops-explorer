using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Data;
using Microsoft.EntityFrameworkCore;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class LatestPipelineTemplate
{
    private readonly AzureDevopsApiProjectClient httpClient;

    public LatestPipelineTemplate(AzureDevopsApiProjectClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task Run(DataConfig config)
    {
        if (config.PipelineRunTemplateImport)
        {
            await RunAddMissingPipelines();
            await RunForOlderAndMissing();
        }
    }

    public async Task RunAddMissingPipelines()
    {
        using var db = new DataContext();

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

        using (var db = new DataContext())
        {
            var imports = await db.LatestPipelineTemplateImport
                .Where(x => x.LastImport == null || x.LastImport < DateTime.UtcNow.AddMonths(-1))
                .Select(x => new { pid = x.PipelineId, pr = x.PipelineRevision })
                .ToListAsync();
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
        using var db = new DataContext();

        var import = await db.LatestPipelineTemplateImport
            .Where(x => x.PipelineId == pipeline.PipelineId && x.PipelineRevision == pipeline.PipelineRevision)
            .SingleAsync();

        var pipelineYaml = await db.Pipeline
            .Where(x => x.Id == pipeline.PipelineId && x.Revision == pipeline.PipelineRevision)
            .SingleAsync();

        import.LastImport = lastImportTime;

        var fileResult = await httpClient.GetFile(pipelineYaml.ConfigurationRepositoryId.Value, pipelineYaml.ConfigurationPath);
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
