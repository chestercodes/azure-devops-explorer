using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Neo4j.NodeLoaders;

namespace AzureDevopsExplorer.Neo4j.RelationshipLoaders;

public class HasCheckRelationships : ILoadRelationshipsToNeo4J
{
    public string Name => "HAS_CHECK";

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        var checkConfigs = db.CheckConfiguration.ToList();
        List<Relationship> relationships = new();
        checkConfigs.ForEach(cc =>
        {
            relationships.Add(new Relationship
            {
                RelationshipName = Name,
                SourceNodeType = ResourceTypeToSourceNodeType(cc.ResourceType),
                SourceMatches = new List<RelationshipMatchPair> {
                    new RelationshipMatchPair { MatchName = ResourceTypeToSourceNodeId(cc.ResourceType), MatchValue = cc.ResourceId.ToString() }
                },
                DestNodeType = CheckConfiguration.Name,
                DestMatches = new List<RelationshipMatchPair> {
                    new RelationshipMatchPair { MatchName = CheckConfiguration.Props.Id, MatchValue = cc.Id.ToString() }
                }
            });
        });
        await loader.LoadRelationships(relationships);
    }

    private static string ResourceTypeToSourceNodeType(string? resourceType)
    {
        switch (resourceType.ToLower())
        {
            case "endpoint": return ServiceEndpoint.Name;
            case "environment": return PipelineEnvironment.Name;
            case "securefile": return SecureFile.Name;
            case "variablegroup": return VariableGroup.Name;
            default: throw new NotImplementedException($"Count not find resource type '{resourceType}'");
        }
    }

    private static string ResourceTypeToSourceNodeId(string? resourceType)
    {
        switch (resourceType.ToLower())
        {
            case "endpoint": return ServiceEndpoint.Props.Id;
            case "environment": return PipelineEnvironment.Props.Id;
            case "securefile": return SecureFile.Props.Id;
            case "variablegroup": return VariableGroup.Props.Id;
            default: throw new NotImplementedException($"Count not find resource type '{resourceType}'");
        }
    }
}
