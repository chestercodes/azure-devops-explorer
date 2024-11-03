using Neo4j.Driver;

namespace AzureDevopsExplorer.Neo4j;

public interface ICreateNeo4jDriver
{
    IDriver Create();
}

public class Neo4jDriverFactory : ICreateNeo4jDriver
{
    private readonly string url;
    private readonly string username;
    private readonly string password;

    public Neo4jDriverFactory(string url, string username, string password)
    {
        this.url = url;
        this.username = username;
        this.password = password;
    }
    public IDriver Create()
    {
        return GraphDatabase.Driver(url, AuthTokens.Basic(username, password));
    }
}