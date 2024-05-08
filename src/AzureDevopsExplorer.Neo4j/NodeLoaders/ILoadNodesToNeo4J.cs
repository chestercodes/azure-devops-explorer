using AzureDevopsExplorer.Database;

namespace AzureDevopsExplorer.Neo4j.NodeLoaders;

public interface ILoadNodesToNeo4J
{
    Task Load(Neo4jLoader loader, DataContext db);
}
