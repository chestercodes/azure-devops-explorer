using AzureDevopsExplorer.IntegrationTests.Fixtures;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace AzureDevopsExplorer.IntegrationTests.Stubs.ApiDtos;

public class BuildArtifactsJsonMapping
{
    public static void Setup(WiremockFixture wiremockFixture, int buildId, BuildArtifactJsonBuilder[] buildArtifactJsons)
    {
        var buildsJson = buildArtifactJsons.Select(x => x.Build());
        var urlPath = $"/{Constants.OrganisationName}/{Constants.ProjectName}/_apis/build/builds/{buildId}/artifacts";
        var json = JsonConvert.SerializeObject(new
        {
            value = buildsJson,
            count = buildsJson.Count()
        });

        wiremockFixture.WireMock.Given(
            Request.Create()
                .WithPath(urlPath)
                .UsingGet()
            )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(json)
            );
    }
}
