namespace AzureDevopsExplorer.AzureDevopsApi.Client;

using Azure.Core;
using Azure.Identity;
using System.Threading;

public class AzureDevopsAccessTokenProvider
{
    public async Task<AccessToken> GetAccessToken(CancellationToken cancellationToken)
    {
        var isLocal = Environment.GetEnvironmentVariable("SET_IF_LOCAL_DEV") != null;
        TokenCredential cred = isLocal ? new AzureCliCredential() : new DefaultAzureCredential();

        var azDoScope = "499b84ac-1321-427f-aa17-267ca6975798";
        var requestContext = new TokenRequestContext([azDoScope]);
        var token = await cred.GetTokenAsync(requestContext, cancellationToken);
        return token;
    }
}