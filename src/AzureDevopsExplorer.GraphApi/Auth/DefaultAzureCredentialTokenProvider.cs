namespace AzureDevopsExplorer.GraphApi.Auth;

using Azure.Core;
using Azure.Identity;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Threading;

public class DefaultAzureCredentialTokenProvider : IAccessTokenProvider
{
    public AllowedHostsValidator AllowedHostsValidator { get; } = new AllowedHostsValidator();

    /// <inheritdoc/>
    public async Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        var isLocal = Environment.GetEnvironmentVariable("SET_IF_LOCAL_DEV") != null;
        TokenCredential cred = isLocal ? new AzureCliCredential() : new DefaultAzureCredential();

        var azDoScope = "https://graph.microsoft.com/.default";
        var requestContext = new TokenRequestContext([azDoScope]);
        var token = await cred.GetTokenAsync(requestContext, cancellationToken);
        return token.Token;
    }
}