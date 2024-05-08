using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Data;

namespace AzureDevopsExplorer.Application.Entrypoints.Evaluate;
public class LatestPipelineAndRun
{
    public async Task Run()
    {
        await RunAddLatestPipeline();
        await RunAddLatestPipelineDefaultRun();
    }

    public async Task RunAddLatestPipeline()
    {
        using var db = new DataContext();

        var latestPipelineAndRevisions = db.Definition
            .GroupBy(dr => dr.Id)
            .Select(group => new
            {
                Id = group.Key,
                Revision = group.Max(dr => dr.Revision),
            })
            .ToList();

        db.LatestPipeline.RemoveRange(db.LatestPipeline);
        foreach (var row in latestPipelineAndRevisions.ToList())
        {
            var def = db.Definition.SingleOrDefault(x => x.Id == row.Id && x.Revision == row.Revision);
            if (def != null)
            {
                db.LatestPipeline.Add(new LatestPipeline
                {
                    Id = def.Id,
                    Revision = def.Revision,
                    Name = def.Name,
                    Path = def.Path,
                    ProjectId = def.ProjectId,
                });
            }
        }

        await db.SaveChangesAsync();
    }


    public async Task RunAddLatestPipelineDefaultRun()
    {
        // add builds which mention environment which are probably deployments and other ones which are probably builds
        using var db = new DataContext();

        var lastBuildsWithEnvParameter = db.Build
            .Join(
                db.BuildTemplateParameter,
                b => b.Id,
                btp => btp.BuildId,
                (b, btp) => new
                {
                    b.Id,
                    b.DefinitionId,
                    b.SourceBranch,
                    btp.Name,
                    btp.Value
                })
            .Where(b => b.SourceBranch == "refs/heads/master" || b.SourceBranch == "refs/heads/main")
            .Where(btp => btp.Name.Contains("environment") && btp.Value.Contains("prod"))
            .GroupBy(b => b.DefinitionId)
            .Select(group => new
            {
                DefinitionId = group.Key,
                MaxId = group.Max(b => b.Id)
            })
            .ToList();

        var alreadySeenPipelineIds = lastBuildsWithEnvParameter.Select(x => x.DefinitionId).ToList();
        var lastBuildsWithDefaultBranch = db.Build
            .Where(b => alreadySeenPipelineIds.Contains(b.DefinitionId) == false)
            .Where(b => b.SourceBranch == "refs/heads/master" || b.SourceBranch == "refs/heads/main")
            .GroupBy(b => b.DefinitionId)
            .Select(group => new
            {
                DefinitionId = group.Key,
                MaxId = group.Max(b => b.Id)
            })
            .ToList();

        db.LatestPipelineDefaultRun.RemoveRange(db.LatestPipelineDefaultRun);
        foreach (var item in lastBuildsWithEnvParameter)
        {
            db.LatestPipelineDefaultRun.Add(new LatestPipelineDefaultRun
            {
                Id = item.MaxId,
                PipelineId = item.DefinitionId
            });
        }
        foreach (var item in lastBuildsWithDefaultBranch)
        {
            db.LatestPipelineDefaultRun.Add(new LatestPipelineDefaultRun
            {
                Id = item.MaxId,
                PipelineId = item.DefinitionId
            });
        }

        await db.SaveChangesAsync();
    }
}
