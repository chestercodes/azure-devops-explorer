using AzureDevopsExplorer.Database;

namespace AzureDevopsExplorer.Neo4j.NodeLoaders;

public class Pipeline : ILoadNodesToNeo4J
{
    public const string Name = "Pipeline";
    public class Props
    {
        public const string Id = "id";
        public const string Name = "name";
        public const string ProjectId = "projectId";
        public const string Folder = "folder";
    }

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        var toAdd =
            db.PipelineRun
            .ToList()
            .Select(x => new Dictionary<string, string> {
                { Props.Id, x.PipelineId.ToString() },
                { Props.Name, x.PipelineName },
                { Props.Folder, x.PipelineFolder }
            })
            .ToArray();

        await loader.DeleteThenLoadNodes(Name, toAdd);
    }
}
