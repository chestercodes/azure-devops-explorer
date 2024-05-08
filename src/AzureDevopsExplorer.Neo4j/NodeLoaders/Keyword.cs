using AzureDevopsExplorer.Database;

namespace AzureDevopsExplorer.Neo4j.NodeLoaders;

public class Keyword : ILoadNodesToNeo4J
{
    public const string Name = "Keyword";
    public class Props
    {
        public const string Key = "key";
        public const string SearchTerm = "searchTerm";
    }

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        var toAdd =
            db.CodeSearchKeyword
            .ToList()
            .Select(x => new Dictionary<string, string> {
                { Props.Key, x.SearchKey },
                { Props.SearchTerm, x.SearchTerm }
            })
            .ToArray();

        await loader.DeleteThenLoadNodes(Name, toAdd);
    }
}
