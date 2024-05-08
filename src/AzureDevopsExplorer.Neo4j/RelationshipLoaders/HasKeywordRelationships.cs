using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Neo4j.NodeLoaders;

namespace AzureDevopsExplorer.Neo4j.RelationshipLoaders;

public class HasKeywordRelationships : ILoadRelationshipsToNeo4J
{
    public string Name => "HAS_KEYWORD";

    public class Props
    {
        public const string FilePath = "filePath";
    }

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        List<Relationship> relationships = new();
        db.CodeSearchKeywordUsage.ToList().ForEach(ku =>
        {
            relationships.Add(new Relationship
            {
                RelationshipName = Name,
                RelationshipProps = new List<RelationshipProperty>
                {
                    new RelationshipProperty(Props.FilePath, ku.FilePath)
                },
                SourceNodeType = Repository.Name,
                SourceMatches = new List<RelationshipMatchPair> {
                    new RelationshipMatchPair { MatchName = Repository.Props.Id, MatchValue = ku.RepositoryId.ToString() }
                },
                DestNodeType = Keyword.Name,
                DestMatches = new List<RelationshipMatchPair> {
                    new RelationshipMatchPair { MatchName = Keyword.Props.Key, MatchValue = ku.SearchKey }
                }
            });
        });
        await loader.LoadRelationships(relationships);
    }
}
