using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Neo4j.NodeLoaders;
using Microsoft.EntityFrameworkCore;

namespace AzureDevopsExplorer.Neo4j.RelationshipLoaders;

public class HasMemberRelationships : ILoadRelationshipsToNeo4J
{
    public string Name => "HAS_MEMBER";

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        var identitysWithMembers = db.Identity.Include(x => x.Members).ToList();
        List<Relationship> relationships = new();
        identitysWithMembers.ForEach(i =>
        {
            i.Members.ForEach(mi =>
            {
                relationships.Add(new Relationship
                {
                    RelationshipName = Name,
                    SourceNodeType = Identity.Name,
                    SourceMatches = new List<RelationshipMatchPair> {
                    new RelationshipMatchPair { MatchName = Identity.Props.Descriptor, MatchValue = i.Descriptor }
                },
                    DestNodeType = Identity.Name,
                    DestMatches = new List<RelationshipMatchPair> {
                    new RelationshipMatchPair {MatchName = Identity.Props.Descriptor, MatchValue = mi.Descriptor}
                }
                });
            });
        });
        await loader.LoadRelationships(relationships);
    }
}
