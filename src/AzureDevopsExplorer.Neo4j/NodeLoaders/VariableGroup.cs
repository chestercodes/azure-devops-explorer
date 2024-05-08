using AzureDevopsExplorer.Database;

namespace AzureDevopsExplorer.Neo4j.NodeLoaders;

public class VariableGroup : ILoadNodesToNeo4J
{
    public const string Name = "VariableGroup";
    public class Props
    {
        public const string Id = "id";
        public const string Name = "name";
    }

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        var toAdd =
            db.VariableGroup
            .ToList()
            .Select(x => new Dictionary<string, string> {
                { Props.Id, x.Id.ToString() },
                { Props.Name, x.Name }
            })
            .ToArray();

        await loader.DeleteThenLoadNodes(Name, toAdd);
    }
}
