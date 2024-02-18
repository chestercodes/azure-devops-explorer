using AzureDevopsExplorer.IntegrationTests.Fixtures;
using WireMock.Admin.Mappings;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace AzureDevopsExplorer.IntegrationTests.Stubs.ApiDtos;

public class ConnectionDataMapping
{
    public static void Setup(WiremockFixture wiremockFixture)
    {
        var json = new ConnectionDataJsonBuilder().AsJson();
        var urlPath = $"/{Constants.OrganisationName}/_apis/connectionData*";

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

    public static void SetupMapping(WiremockFixture wiremockFixture)
    {
        var json = new ConnectionDataJsonBuilder().AsJson();
        var urlPath = $"/{Constants.OrganisationName}/_apis/connectionData*";

        var mappingModel = new MappingModel
        {
            Guid = null,
            Request = new RequestModel
            {
                Methods = new[] { "GET" },
                Path = urlPath,
            },
            Response = new ResponseModel
            {
                StatusCode = 200,
                Headers = new Dictionary<string, object>
                {
                    ["Content-Type"] = "application/json; charset=utf-8"
                },
                Body = json,
            }
        };

        var status = wiremockFixture.AdminApi.PostMappingAsync(mappingModel).Result;
    }
}
