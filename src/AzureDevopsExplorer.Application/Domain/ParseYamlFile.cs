using OneOf;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Newtonsoft.Json;

namespace AzureDevopsExplorer.Application.Domain;

public class ParseYamlFile
{
    private void ConvertChildNodes(YamlMappingNode node, Dictionary<string, string> dict, string parentKey = "$")
    {
        foreach (var child in node.Children)
        {
            var key = child.Key.ToString();
            var value = child.Value.ToString();

            if (child.Value is YamlScalarNode)
            {
                dict.Add($"{parentKey}.{key}", value);

            }
            else if (child.Value is YamlMappingNode)
            {
                var newKey = $"{parentKey}.{key}";
                ConvertChildNodes((YamlMappingNode)child.Value, dict, newKey);
            }
            else if (child.Value is YamlSequenceNode)
            {
                var i = 0;
                var seq = child.Value as YamlSequenceNode;
                foreach (var item in seq.Children)
                {
                    var newKey = $"{parentKey}.{key}.[{i}]";
                    if (item is YamlScalarNode)
                    {
                        var scalarItem = (YamlScalarNode)item;
                        dict.Add(newKey, scalarItem.Value.ToString());
                    }
                    else if (item is YamlMappingNode)
                    {
                        ConvertChildNodes((YamlMappingNode)item, dict, newKey);
                    }

                    i++;
                }
            }
        }
    }

    public OneOf<TemplateExtendsParseResult, Exception> ParsePipelineYamlForExtends(string yamlFile)
    {
        try
        {
            if (yamlFile.Contains("extends:") == false)
            {
                return new TemplateExtendsParseResult(false, null);
            }
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            //yml contains a string containing your YAML
            var y = deserializer.Deserialize<YamlTemplateExtends>(yamlFile);
            if (y?.Extends?.Template == null)
            {
                return new TemplateExtendsParseResult(false, null);
            }

            if (y?.Resources?.Repositories?.Count == null || y?.Resources?.Repositories?.Count == 0)
            {
                return new TemplateExtendsParseResult(false, null);
            }

            var extendsTemplate = y.Extends.Template;
            var spl = extendsTemplate.Split('@');
            if (spl.Length == 1)
            {
                return new TemplateExtendsParseResult(false, null);
            }

            var repoPath = spl[0];
            var repoRef = spl[1];
            var matchingRepo = y.Resources.Repositories.Where(x => x.Repository == repoRef);
            if (matchingRepo.Count() == 0)
            {
                return new TemplateExtendsParseResult(false, null);
            }

            var repo = matchingRepo.First();
            return new TemplateExtendsParseResult(true, new TemplateExtendsInfo(repo.Name, repoPath, repo.Ref));
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public OneOf<TemplateSchedulesParseResult, Exception> ParsePipelineYamlForSchedules(string yamlFile)
    {
        try
        {
            if (yamlFile.Contains("schedules:") == false)
            {
                return new TemplateSchedulesParseResult(false, null);
            }
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            //yml contains a string containing your YAML
            var y = deserializer.Deserialize<YamlTemplateSchedules>(yamlFile);
            if (y?.Schedules == null)
            {
                return new TemplateSchedulesParseResult(false, null);
            }

            var schedules = JsonConvert.SerializeObject(y.Schedules);
            return new TemplateSchedulesParseResult(true, schedules);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public OneOf<ParsedYamlFile, Exception> ParseExpandedBuildYaml(string yamlFile)
    {
        try
        {
            var input = new StringReader(yamlFile);

            var yaml = new YamlStream();
            yaml.Load(input);

            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            var dictionary = new Dictionary<string, string>();

            ConvertChildNodes(mapping, dictionary);

            var serviceConnections = new List<ServiceConnectionUsage>();
            foreach (var kvp in dictionary)
            {
                var k = kvp.Key;
                var v = kvp.Value;
                var isServiceConnection = k.EndsWith("inputs.azureSubscription") || k.EndsWith("inputs.connectedServiceNameARM"); // || k.EndsWith("inputs.serviceConnection");
                if (isServiceConnection)
                {
                    serviceConnections.Add(new ServiceConnectionUsage(v, k));
                }
            }

            var variableGroups = new List<VariableGroupUsage>();
            foreach (var kvp in dictionary)
            {
                var k = kvp.Key;
                var v = kvp.Value;
                var isVariableGroup = Regex.IsMatch(k, ".*\\.variables\\.\\[\\d+\\]\\.group");
                if (isVariableGroup)
                {
                    variableGroups.Add(new VariableGroupUsage(v, k));
                }
            }

            var specificVariables = new List<SpecificVariableUsage>();

            var variableNameKeys = dictionary.Keys.Where(x => Regex.IsMatch(x, ".*\\.variables\\.\\[\\d+\\]\\.name"));
            foreach (var varNameKey in variableNameKeys)
            {
                var valueKey = varNameKey.Substring(0, varNameKey.Length - "name".Length) + "value";
                var key = dictionary[varNameKey];
                var value = dictionary[valueKey];
                specificVariables.Add(new SpecificVariableUsage(key, value, varNameKey));
            }

            var pipelineEnvironments = new List<PipelineEnvironmentUsage>();

            foreach (var kvp in dictionary)
            {
                var k = kvp.Key;
                var v = kvp.Value;
                var isEnvironmentName = k.EndsWith("environment.name");
                if (isEnvironmentName)
                {
                    pipelineEnvironments.Add(new PipelineEnvironmentUsage(v, k));
                }
            }

            return new ParsedYamlFile(
                specificVariables,
                variableGroups,
                serviceConnections,
                pipelineEnvironments);
        }
        catch (Exception e)
        {
            return e;
        }
    }
}

public class YamlTemplateExtends
{
    public class RepositoryObj
    {
        public string? Repository { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Ref { get; set; }
    }
    public class ResourcesObj
    {
        public List<RepositoryObj>? Repositories { get; set; }
    }
    public class ExtendsObj
    {
        public string Template { get; set; }
    }

    public ExtendsObj Extends { get; set; }
    public ResourcesObj Resources { get; set; }
}

public class YamlTemplateSchedules
{
    public class Schedule
    {
        public string? Cron { get; set; }
        public string? DisplayName { get; set; }
    }

    public Schedule[] Schedules { get; set; }
}

public record TemplateExtendsInfo(
    string Repository,
    string Path,
    string Branch
    );
public record TemplateExtendsParseResult(
    bool TemplateExtendsFromRepository,
    TemplateExtendsInfo? Info
    );

public record TemplateSchedulesParseResult(
    bool TemplateIsOnSchedule,
    string? Data
    );

public record ParsedYamlFile(
    List<SpecificVariableUsage> SpecificVariables,
    List<VariableGroupUsage> VariableGroups,
    List<ServiceConnectionUsage> ServiceConnections,
    List<PipelineEnvironmentUsage> PipelineEnvironments
    );

public record PipelineEnvironmentUsage(
    string Name,
    string Location
    );

public record SpecificVariableUsage(
    string Name,
    string Value,
    string Location
    );

public record VariableGroupUsage(
    string Name,
    string Location
    );

public record ServiceConnectionUsage(
    string Name,
    string Location
    );