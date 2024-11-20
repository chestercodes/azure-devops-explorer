using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Pipelines;

namespace AzureDevopsExplorer.Application.Entrypoints.Evaluate;
public class LatestPipelineAndRun
{
    private readonly ICreateDataContexts dataContextFactory;

    public LatestPipelineAndRun(ICreateDataContexts dataContextFactory)
    {
        this.dataContextFactory = dataContextFactory;
    }

    public async Task Run(ProcessConfig config)
    {
        if (config.UpdateLatestPipelineAndRun)
        {
            await RunAddLatestPipeline();
            await RunAddLatestPipelineDefaultRun();
        }
    }

    public async Task RunAddLatestPipeline()
    {
        using var db = dataContextFactory.Create();

        db.PipelineLatest.RemoveRange(db.PipelineLatest);
        foreach (var row in db.PipelineCurrent.ToList())
        {
            var def = db.Pipeline.SingleOrDefault(x => x.Id == row.Id && x.Revision == row.Revision);
            if (def != null)
            {
                db.PipelineLatest.Add(new PipelineLatest
                {
                    Id = def.Id,
                    Revision = def.Revision,
                    Name = def.Name,
                    Folder = def.Folder,
                    ProjectId = def.ProjectId,
                });
            }
        }

        await db.SaveChangesAsync();
    }


    public async Task RunAddLatestPipelineDefaultRun()
    {
        // add builds which mention environment which are probably deployments and other ones which are probably builds
        using var db = dataContextFactory.Create();

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

        db.PipelineLatestDefaultBranchRun.RemoveRange(db.PipelineLatestDefaultBranchRun);
        foreach (var item in lastBuildsWithEnvParameter)
        {
            db.PipelineLatestDefaultBranchRun.Add(new PipelineLatestDefaultBranchRun
            {
                Id = item.MaxId,
                PipelineId = item.DefinitionId
            });
        }
        foreach (var item in lastBuildsWithDefaultBranch)
        {
            db.PipelineLatestDefaultBranchRun.Add(new PipelineLatestDefaultBranchRun
            {
                Id = item.MaxId,
                PipelineId = item.DefinitionId
            });
        }

        await db.SaveChangesAsync();
    }
}
