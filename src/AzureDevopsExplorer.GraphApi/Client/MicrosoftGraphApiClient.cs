using Azure.Identity;
using AzureDevopsExplorer.GraphApi.Auth;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;

namespace AzureDevopsExplorer.GraphApi.Client;

public class MicrosoftGraphApiClient
{
    public GraphServiceClient Value => new GraphServiceClient(new DefaultAzureCredential(), new[] { "https://graph.microsoft.com/.default" });
    public GraphServiceClient ValueHard => new GraphServiceClient(new BaseBearerTokenAuthenticationProvider(new DefaultAzureCredentialTokenProvider()));
}
