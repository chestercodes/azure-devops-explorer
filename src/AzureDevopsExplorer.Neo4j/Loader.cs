using Neo4j.Driver;

namespace AzureDevopsExplorer.Neo4j;

public class Neo4jLoader
{
    private readonly IDriver driver;

    public Neo4jLoader(IDriver driver)
    {
        this.driver = driver;
    }

    public async Task DeleteAllRelationships(List<string> relationshipTypes)
    {
        await using var session = driver.AsyncSession();
        await session.ExecuteWriteAsync(
            async tx =>
            {
                foreach (var item in relationshipTypes)
                {
                    await tx.RunAsync($"MATCH ()-[r:{item}]->() DELETE r ");
                }
            });
    }

    public async Task DeleteThenLoadNodes(string nodeType, Dictionary<string, string>[] nodes)
    {
        await using var session = driver.AsyncSession();
        await session.ExecuteWriteAsync(
            async tx =>
            {
                var props = new Dictionary<string, object>()
                {
                        {"props", nodes }
                };
                await tx.RunAsync($"MATCH (n:{nodeType} ) DELETE n ");
                await tx.RunAsync($"UNWIND $props AS map CREATE (n:{nodeType}) SET n = map", props);
            });
    }

    public async Task LoadRelationships(IEnumerable<Relationship> relationships)
    {
        await using var session = driver.AsyncSession();
        await session.ExecuteWriteAsync(
            async tx =>
            {
                foreach (var relationship in relationships)
                {
                    var r = relationship;
                    var sourceMatches = string.Join(" AND ", r.SourceMatches.Select(m => $" s.{m.MatchName} = \"{CleanValue(m.MatchValue)}\" "));
                    var destMatches = string.Join(" AND ", r.DestMatches.Select(m => $" d.{m.MatchName} = \"{CleanValue(m.MatchValue)}\" "));
                    var relationshipProperties = relationship.RelationshipProps.Count == 0
                        ? ""
                        : "{ " + string.Join(" , ", r.RelationshipProps.Select(p => $" {p.Key}: \"{CleanValue(p.Value)}\" ")) + " }";
                    var query = @$"MATCH (s:{r.SourceNodeType}),(d:{r.DestNodeType}) WHERE {sourceMatches} AND {destMatches} CREATE (s)-[ :{r.RelationshipName} {relationshipProperties} ]->(d)
";
                    await tx.RunAsync(query);
                }
            });

    }

    private string CleanValue(string v)
    {
        return v.Replace("\\", "\\\\");
    }
}
