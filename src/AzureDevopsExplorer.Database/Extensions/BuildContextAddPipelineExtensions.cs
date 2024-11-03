using AzureDevopsExplorer.AzureDevopsApi.Pipelines;

namespace AzureDevopsExplorer.Database.Extensions;

public static class BuildContextAddPipelineExtensions
{
    private static Mappers.Mappers mapper = new Mappers.Mappers();

    public static DataContext AddPipelineRun(this DataContext db, PipelineRun run)
    {
        var pipelineRun = mapper.MapPipelineRun(run);
        if (run.resources.pipelines != null)
        {
            foreach (var pipeline in run.resources.pipelines)
            {
                pipelineRun.ResourcesPipelines.Add(new Model.Pipelines.PipelineRunPipelineInfo
                {
                    PipelineRunId = run.id,
                    ResourceRefName = pipeline.Key,
                    Name = pipeline.Value.pipeline?.name,
                    Revision = pipeline.Value.pipeline?.revision,
                    //Url = pipeline.Value.Pipeline?.Url,
                    Folder = pipeline.Value.pipeline?.folder,
                    Version = pipeline.Value.version
                });
            }
        }
        if (run.resources.repositories != null)
        {
            foreach (var repo in run.resources.repositories)
            {
                pipelineRun.ResourcesRepositories.Add(new Model.Pipelines.PipelineRunRepositoryInfo
                {
                    PipelineRunId = run.id,
                    ResourceRefName = repo.Key,
                    RefName = repo.Value.refName,
                    RepositoryId = repo.Value.repository.id,
                    RepositoryType = repo.Value.repository.type,
                    Version = repo.Value.version,
                });
            }
        }

        db.PipelineRun.Add(pipelineRun);


        return db;
    }
}
