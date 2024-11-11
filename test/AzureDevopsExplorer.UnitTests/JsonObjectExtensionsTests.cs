using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.Neo4j;
using AzureDevopsExplorer.Neo4j.NodeLoaders;
using System.Text.Json;

namespace AzureDevopsExplorer.UnitTests;

public class JsonObjectExtensionsTests
{
    [Fact]
    public void WritesOutFlattenedProperties()
    {
        var json = @"
 {
        ""maximumGitBlobSizeInBytes"": 262,
        ""useUncompressedSize"": false,
        ""scope"": [
          {
            ""repositoryId"": ""repo1""
          },
          {
            ""branch"": ""ref/heads/main"",
            ""repositoryId"": ""repo2""
          }
        ]
      }
";
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

        var result = dict.FlattenJsonObject();

        var pause = 0;
    }
}