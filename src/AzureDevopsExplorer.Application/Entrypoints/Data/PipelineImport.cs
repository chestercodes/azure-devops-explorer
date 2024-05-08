using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Extensions;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using Microsoft.EntityFrameworkCore;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class PipelineImportCmd
{
    private readonly AzureDevopsApiProjectClient httpClient;
    private readonly Mappers mapper;

    public PipelineImportCmd(AzureDevopsApiProjectClient httpClient)
    {
        this.httpClient = httpClient;
        mapper = new Mappers();
    }

    public async Task Run(DataConfig config)
    {
        if (config.PipelineRunImport)
        {
            await RunAddMissingPipelinesToImport();
            await RunDownloadPipeline();
        }
    }

    public async Task RunAddMissingPipelinesToImport()
    {
        using var db = new DataContext();

        var missingInImportsTable =
            from d in db.Definition
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

        using (var db = new DataContext())
        {
            var imports = await db.PipelineImport
                .Where(x => x.PipelineImportState == PipelineImportState.Initial)
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
        using var db = new DataContext();

        var queries = new AzureDevopsApiProjectQueries(httpClient);

        var pipelineImport = await db.PipelineImport.Where(x => x.PipelineId == pipelineId && x.PipelineRevision == pipelineRevision).SingleOrDefaultAsync();
        if (pipelineImport != null)
        {
            var pipelineResult = await queries.GetPipeline(pipelineId, pipelineRevision);
            pipelineResult.Switch(pipeline =>
            {
                var pipelineYaml = mapper.MapPipeline(pipeline);
                db.Pipeline.Add(pipelineYaml);

                foreach (var variable in pipeline?.Configuration?.Variables ?? [])
                {
                    db.PipelineVariable.Add(new PipelineVariable
                    {
                        PipelineId = pipelineId,
                        PipelineRevision = pipelineRevision,
                        Name = variable.Key,
                        Value = variable.Value.Value,
                        IsSecret = variable.Value.IsSecret,
                    });
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
            Console.WriteLine($"Pipeline Id {pipelineId} {pipelineRevision}, this shouldn't happen...");
        }
    }
}
