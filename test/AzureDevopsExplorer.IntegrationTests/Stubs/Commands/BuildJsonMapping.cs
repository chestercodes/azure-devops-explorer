using AzureDevopsExplorer.IntegrationTests.Fixtures;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace AzureDevopsExplorer.IntegrationTests.Stubs.ApiDtos;

public class BuildJsonMapping
{
    public static void Setup(WiremockFixture wiremockFixture, BuildJsonBuilder buildJson)
    {
        var buildJsonObj = buildJson.Value;
        var urlPath = $"/{Constants.OrganisationName}/{Constants.ProjectName}/_apis/build/builds/{buildJsonObj.id}";

        wiremockFixture.WireMock.Given(
            Request.Create()
                .WithPath(urlPath)
                .UsingGet()
            )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(buildJson.AsJson())
            );
    }
}
