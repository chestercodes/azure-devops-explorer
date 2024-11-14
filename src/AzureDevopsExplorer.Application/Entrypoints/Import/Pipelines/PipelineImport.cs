using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi.Pipelines;
using AzureDevopsExplorer.Database.Extensions;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Pipelines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Pipelines;
public class PipelineImportCmd
{
    private readonly ILogger logger;
    private readonly Mappers mapper;
    private readonly AzureDevopsProjectDataContext dataContext;

    public PipelineImportCmd(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.Create(this);
        mapper = new Mappers();
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.PipelineImport)
        {
            await RunAddMissingPipelinesToImport();
            await RunDownloadPipeline();
        }
    }

    public async Task RunAddMissingPipelinesToImport()
    {
        using var db = dataContext.DataContextFactory.Create();

        var missingInImportsTable =
            from d in db.PipelineCurrent
            join p in db.PipelineImport on d.Id equals p.PipelineId into g
            from p in g.DefaultIfEmpty()
            where p == null
            select new { id = d.Id, revision = d.Revision };

        foreach (var row in missingInImportsTable.ToList())
        {
            db.PipelineImport.Add(new PipelineImport
            {
                PipelineId = row.id,
                PipelineRevision = row.revision,
                PipelineImportState = PipelineImportState.Initial
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task RunDownloadPipeline()
    {
        List<(int, int)> pipelineImports = new();

        using (var db = dataContext.DataContextFactory.Create())
        {
            var imports = await db.PipelineImport
                .Where(
                    x => x.PipelineImportState == PipelineImportState.Initial &&
                        db.PipelineCurrent
                        .Where(y => y.ProjectId == dataContext.Project.ProjectId)
                        .Select(y => y.Id)
                        .Contains(x.PipelineId)
                )
                .Select(x => new { pipelineId = x.PipelineId, pipelineRevision = x.PipelineRevision })
                .ToListAsync();
            pipelineImports = imports.Select(x => (x.pipelineId, x.pipelineRevision)).ToList();
        }

        foreach (var pipeline in pipelineImports)
        {
            await RunForPipelineId(pipeline.Item1, pipeline.Item2);
        }
    }

    private async Task RunForPipelineId(int pipelineId, int pipelineRevision)
    {
        logger.LogDebug($"Running pipeline import " + pipelineId);

        using var db = dataContext.DataContextFactory.Create();

        var pipelineImport = await db.PipelineImport.Where(x => x.PipelineId == pipelineId && x.PipelineRevision == pipelineRevision).SingleOrDefaultAsync();
        if (pipelineImport != null)
        {
            var pipelineResult = await dataContext.Queries.Pipelines.GetPipeline(pipelineId, pipelineRevision);
            pipelineResult.Switch(pipeline =>
            {
                if (pipeline.IsT0)
                {
                    var pipelineYamlFromApi = pipeline.AsT0;
                    var pipelineYaml = mapper.MapPipelineYaml(pipelineYamlFromApi);
                    pipelineYaml.ProjectId = dataContext.Project.ProjectId;
                    db.Pipeline.Add(pipelineYaml);

                    foreach (var variable in pipelineYamlFromApi?.configuration?.variables ?? [])
                    {
                        db.PipelineVariable.Add(new PipelineVariable
                        {
                            PipelineId = pipelineId,
                            PipelineRevision = pipelineRevision,
                            Name = variable.Key,
                            Value = variable.Value.value,
                            IsSecret = variable.Value.isSecret,
                        });
                    }
                }
                else
                {
                    var pipelineSimple = mapper.MapPipelineSimple(pipeline.AsT1);
                    pipelineSimple.ProjectId = dataContext.Project.ProjectId;
                    db.Pipeline.Add(pipelineSimple);
                }

                pipelineImport.PipelineImportState = PipelineImportState.Done;
                db.SaveChanges();
            }, err =>
            {
                var errorHash = db.AddImportError(err.AsError);
                pipelineImport.PipelineImportErrorHash = errorHash;
                pipelineImport.PipelineImportState = PipelineImportState.ErrorFromApi;
                db.SaveChanges();
            });
        }
        else
        {
            logger.LogWarning($"Pipeline Id {pipelineId} {pipelineRevision}, this shouldn't happen...");
        }
    }
}
