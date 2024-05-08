using AzureDevopsExplorer.Database;
using System.Text.Json;

namespace AzureDevopsExplorer.Neo4j.NodeLoaders;

public class CheckConfiguration : ILoadNodesToNeo4J
{
    public const string Name = "CheckConfiguration";
    public class Props
    {
        public const string Id = "id";
        public const string Settings = "settings";
    }

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        var checkConfigs = db.CheckConfiguration.ToList();
        var checkConfigsVals = checkConfigs.Select(x => ParseCheckConfiguration(x));
        await loader.DeleteThenLoadNodes(Name, checkConfigsVals.ToArray());
    }

    private static Dictionary<string, string> ParseCheckConfiguration(Database.Model.Data.CheckConfiguration c)
    {
        var toReturn = new Dictionary<string, string>{
            { Props.Id, c.Id.ToString() },
            { Props.Settings, c.Settings }
        };

        if (c.Settings != "null")
        {
            var settings = JsonSerializer.Deserialize<CheckConfigurationSettings>(c.Settings);
            var settingsProperties = settings.FlattenToDictionary("settings");
            return new Dictionary<string, string>(toReturn.ToArray().Concat(settingsProperties.ToArray()));
        }
        return toReturn;
    }
}
