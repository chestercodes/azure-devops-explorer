//using AzureDevopsExplorer.IntegrationTests.Fixtures;
//using Newtonsoft.Json;
//using WireMock.RequestBuilders;
//using WireMock.ResponseBuilders;

//namespace AzureDevopsExplorer.IntegrationTests.Stubs.ApiDtos;

//public class BuildTimelineJsonMapping
//{
//    public static void Setup(WiremockFixture wiremockFixture, int buildId, BuildTimelineJsonBuilder buildTimelineBuilder)
//    {
//        var jsonObj = buildTimelineBuilder.Value;
//        var urlPath = $"/{Constants.OrganisationName}/{Constants.ProjectName}/_apis/build/builds/{buildId}/Timeline";
//        var json = JsonConvert.SerializeObject(jsonObj);

//        wiremockFixture.WireMock.Given(
//            Request.Create()
//                .WithPath(urlPath)
//                .UsingGet()
//            )
//            .RespondWith(
//                Response.Create()
//                    .WithStatusCode(200)
//                    .WithHeader("Content-Type", "application/json")
//                    .WithBody(json)
//            );
//    }
//}
