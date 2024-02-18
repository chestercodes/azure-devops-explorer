namespace AzureDevopsExplorer.Database.Extensions;

public static class BuildContextAddPipelineExtensions
{
    private static Mappers.Mappers mapper = new Mappers.Mappers();

    public static DataContext AddPipelineRun(this DataContext db, AzureDevopsApi.Dtos.PipelineRun run)
    {
        var pipelineRun = mapper.MapPipelineRun(run);
        if (run.Resources.Pipelines != null)
        {
            foreach (var pipeline in run.Resources.Pipelines)
            {
                pipelineRun.ResourcesPipelines.Add(new Model.Data.PipelineRunPipelineInfo
                {
                    PipelineRunId = run.Id,
                    ResourceRefName = pipeline.Key,
                    Name = pipeline.Value.Pipeline?.Name,
                    Revision = pipeline.Value.Pipeline?.Revision,
                    //Url = pipeline.Value.Pipeline?.Url,
                    Folder = pipeline.Value.Pipeline?.Folder,
                    Version = pipeline.Value.Version
                });
            }
        }
        if (run.Resources.Repositories != null)
        {
            foreach (var repo in run.Resources.Repositories)
            {
                pipelineRun.ResourcesRepositories.Add(new Model.Data.PipelineRunRepositoryInfo
                {
                    PipelineRunId = run.Id,
                    ResourceRefName = repo.Key,
                    RefName = repo.Value.RefName,
                    RepositoryId = repo.Value.Repository.Id,
                    RepositoryType = repo.Value.Repository.Type,
                    Version = repo.Value.Version,
                });
            }
        }

        db.PipelineRun.Add(pipelineRun);


        return db;
    }
}
