namespace AzureDevopsExplorer.Neo4j;



public class RelationshipMatchPair
{
    public string MatchName { get; set; }
    public string MatchValue { get; set; }
}

public class Relationship
{
    public string RelationshipName { get; set; }
    public string SourceNodeType { get; set; }
    public List<RelationshipMatchPair> SourceMatches { get; set; }
    public string DestNodeType { get; set; }
    public List<RelationshipMatchPair> DestMatches { get; set; }
}
