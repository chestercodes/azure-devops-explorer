using RestEase;
using System.Net;
using WireMock.Client;
using WireMock.Server;

namespace AzureDevopsExplorer.IntegrationTests.Fixtures;

public class WiremockFixture : IDisposable
{
    public WireMockServer WireMock { get; private set; }
    public IWireMockAdminApi AdminApi { get; private set; }

    public WiremockFixture()
    {
        WireMock = WireMockServer.StartWithAdminInterface(port: 1080, ssl: true);

        ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => true;

        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (a, b, c, d) => true;

        AdminApi = RestClient.For<IWireMockAdminApi>("https://localhost:1080", handler);
    }

    public void Dispose()
    {
        WireMock.Dispose();
    }
}
