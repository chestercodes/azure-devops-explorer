//using AzureDevopsExplorer.IntegrationTests.Fixtures;
//using Newtonsoft.Json;
//using WireMock.RequestBuilders;
//using WireMock.ResponseBuilders;

//namespace AzureDevopsExplorer.IntegrationTests.Stubs.ApiDtos;

//public class BuildJsonListMapping
//{
//    public static void Setup(WiremockFixture wiremockFixture, BuildJsonBuilder[] buildJsons)
//    {
//        var buildsJson = buildJsons.Select(x => x.Value);
//        var urlPath = $"/{Constants.OrganisationName}/{Constants.ProjectName}/_apis/build/Builds";
//        var json = JsonConvert.SerializeObject(new
//        {
//            value = buildsJson,
//            count = buildsJson.Count()
//        });

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
