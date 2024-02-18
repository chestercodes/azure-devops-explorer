namespace AzureDevopsExplorer.IntegrationTests.Stubs.ApiDtos;

using Newtonsoft.Json;
using static Constants;

public class BuildArtifactJsonBuilder
{
    public BuildArtifactJson Value { get; private set; }

    public BuildArtifactJson Build()
    {
        return Value;
    }
    public string AsJson()
    {
        return JsonConvert.SerializeObject(Value);
    }

    public BuildArtifactJsonBuilder(int buildArtifactId, string buildArtifactName, int buildId)
    {
        Value = new BuildArtifactJson
        {
            id = buildArtifactId,
            name = buildArtifactName,
            resource = new Resource
            {
                type = "PipelineArtifact",
                data = $"SOMEARTIFACTHASHVALUEFOR{buildArtifactId}",
                downloadUrl = $"https://artproduks1.artifacts.visualstudio.com/A7a38dd89-8eb1-4a06-a367-d27510a580ef/{ProjectId}/_apis/artifact/someotherartifacthashid{buildArtifactId}/content?format=zip",
                url = $"https://dev.azure.com/{OrganisationName}/{ProjectId}/_apis/build/builds/{buildId}/artifacts?artifactName={buildArtifactName}&api-version=7.1",
                properties = new Properties
                {
                    artifactsize = new Random().Next(10000, 100000).ToString(),
                    HashType = "DEDUPNODEORCHUNK",
                    RootId = $"SOMEOTHERHASHVALUE{buildArtifactId}"
                }
            }
        };
    }



    public class BuildArtifactJson
    {
        public int id { get; set; }
        public string name { get; set; }
        public Guid source { get; set; }
        public Resource resource { get; set; }
    }

    public class Resource
    {
        public string type { get; set; }
        public string data { get; set; }
        public Properties properties { get; set; }
        public string url { get; set; }
        public string downloadUrl { get; set; }
    }

    public class Properties
    {
        public string RootId { get; set; }
        public string artifactsize { get; set; }
        public string HashType { get; set; }
    }

}
