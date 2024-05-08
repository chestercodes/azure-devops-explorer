using AzureDevopsExplorer.Database;

namespace AzureDevopsExplorer.Neo4j.NodeLoaders;

public class Repository : ILoadNodesToNeo4J
{
    public const string Name = "Repository";
    public class Props
    {
        public const string Id = "id";
        public const string Name = "name";
        public const string ProjectId = "projectId";
    }

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        var toAdd = db.GitRepository
            .ToList()
            .Select(x => new Dictionary<string, string> {
                { Props.Id, x.Id.ToString() },
                { Props.Name, x.Name },
                { Props.ProjectId, x.ProjectReferenceId.ToString() }
            })
            .ToArray();

        await loader.DeleteThenLoadNodes(Name, toAdd);
    }
}
