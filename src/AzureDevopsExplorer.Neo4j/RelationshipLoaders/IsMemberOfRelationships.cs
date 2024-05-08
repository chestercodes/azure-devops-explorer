using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Neo4j.NodeLoaders;
using Microsoft.EntityFrameworkCore;

namespace AzureDevopsExplorer.Neo4j.RelationshipLoaders;

public class IsMemberOfRelationships : ILoadRelationshipsToNeo4J
{
    public string Name => "IS_MEMBER_OF";

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        var identitysWithMembersOf = db.Identity.Include(x => x.MemberOf).ToList();
        List<Relationship> relationships = new();
        identitysWithMembersOf.ForEach(i =>
        {
            i.MemberOf.ForEach(mi =>
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
