using Neo4j.Driver;

namespace AzureDevopsExplorer.Neo4j;

public interface IHaveDriver
{
    IDriver Driver { get; }
}

public class Neo4jDriver : IDisposable, IHaveDriver
{
    private readonly IDriver _driver;

    IDriver IHaveDriver.Driver => _driver;

    public Neo4jDriver()
    {
        string user = Environment.GetEnvironmentVariable("NEO4J_USERNAME") ?? "neo4j";
        string password = Environment.GetEnvironmentVariable("NEO4J_PASSWORD") ?? "somepassword";
        string uri = Environment.GetEnvironmentVariable("NEO4J_URL") ?? "bolt://localhost:7687";
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }

    public void Dispose()
    {
        _driver?.Dispose();
    }
}