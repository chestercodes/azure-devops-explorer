namespace AzureDevopsExplorer.Neo4j;



public class RelationshipMatchPair
{
    public string MatchName { get; set; }
    public string MatchValue { get; set; }
}

public record RelationshipProperty(string Key, string Value);

public class Relationship
{
    public string RelationshipName { get; set; }
    public List<RelationshipProperty> RelationshipProps { get; set; } = new();
    public string SourceNodeType { get; set; }
    public List<RelationshipMatchPair> SourceMatches { get; set; }
    public string DestNodeType { get; set; }
    public List<RelationshipMatchPair> DestMatches { get; set; }
}
