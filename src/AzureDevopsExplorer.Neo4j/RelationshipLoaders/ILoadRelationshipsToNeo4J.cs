using AzureDevopsExplorer.Database;

namespace AzureDevopsExplorer.Neo4j.RelationshipLoaders;

public interface ILoadRelationshipsToNeo4J
{
    string Name { get; }
    Task Load(Neo4jLoader loader, DataContext db);
}