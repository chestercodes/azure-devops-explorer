namespace AzureDevopsExplorer.AzureDevopsApi.Auth;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

public class Connection
{
    public static VssConnection GetFakeConnection()
    {
        var settings = new VssClientHttpRequestSettings
        {
            ServerCertificateValidationCallback = (a, b, c, d) => true,
        };
        var orgUrl = new Uri("https://127.0.0.1:1080/evilcorp");
        var connection = new VssConnection(orgUrl, new VssBasicCredential(string.Empty, "personalAccessToken"), settings);
        return connection;
    }
}
