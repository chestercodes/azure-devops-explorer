using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Neo4j;
using AzureDevopsExplorer.Neo4j.NodeLoaders;
using AzureDevopsExplorer.Neo4j.RelationshipLoaders;

namespace AzureDevopsExplorer.Application.Entrypoints.Loader;

public class LoadLatest
{
    private readonly ICreateDataContexts dataContexts;
    private readonly ICreateNeo4jDriver neo4JDriver;
    private readonly CancellationToken cancellationToken;

    public LoadLatest(ICreateDataContexts dataContexts, ICreateNeo4jDriver neo4jDriver, CancellationToken cancellationToken)
    {
        this.dataContexts = dataContexts;
        neo4JDriver = neo4jDriver;
        this.cancellationToken = cancellationToken;
    }
    public async Task Run()
    {
        using var driver = neo4JDriver.Create();
        var loader = new Neo4jLoader(driver);

        var nodeLoaders = new List<ILoadNodesToNeo4J>
        {
            new CheckConfiguration(),
            new Keyword(),
            new Identity(),
            new PipelineEnvironment(),
            new Pipeline(),
            new Repository(),
            new SecureFile(),
            new ServiceEndpoint(),
            new VariableGroup(),
        };

        var relationshipLoaders = new List<ILoadRelationshipsToNeo4J>
        {
            new HasPermissionRelationships(),
            new HasCheckRelationships(),
            new HasKeywordRelationships(),
            new PipelineConsumesRelationships(),
            new PipelineUsesRelationships(),
            new HasMemberRelationships(),
            new IsMemberOfRelationships(),
        };

        using (var db = dataContexts.Create())
        {
            await DeleteRelationships(loader, relationshipLoaders);

            foreach (var nodeLoader in nodeLoaders)
            {
                await nodeLoader.Load(loader, db);
            }

            foreach (var relationshipLoader in relationshipLoaders)
            {
                await relationshipLoader.Load(loader, db);
            }
        }
    }

    private static async Task DeleteRelationships(Neo4jLoader loader, List<ILoadRelationshipsToNeo4J> relationships)
    {
        var toDelete = relationships.Select(x => x.Name).ToList();
        await loader.DeleteAllRelationships(toDelete);
    }
}
