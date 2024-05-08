using AzureDevopsExplorer.Neo4j;
using AzureDevopsExplorer.Neo4j.NodeLoaders;

namespace AzureDevopsExplorer.UnitTests;

public class ObjectExtensionsTests
{
    [Fact]
    public void WritesOutFlattenedProperties()
    {
        var obj = new CheckConfigurationSettings
        {
            definitionRef = new Definitionref
            {
                id = "1",
                version = "2"
            },
            extendsChecks = new Extendscheck[]
            {
                new Extendscheck
                {
                    repositoryName = "repo",
                    repositoryRef = "main",
                    repositoryType = "git",
                }
            }
        };


        var result = obj.FlattenToDictionary();

        var pause = 0;
    }
}