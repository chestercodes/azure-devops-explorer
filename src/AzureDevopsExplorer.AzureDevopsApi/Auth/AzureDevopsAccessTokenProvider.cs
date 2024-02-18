namespace AzureDevopsExplorer.AzureDevopsApi.Auth;

using Azure.Core;
using Azure.Identity;
using System.Threading;

public class AzureDevopsAccessTokenProvider
{
    public async Task<AccessToken> GetAccessToken(CancellationToken cancellationToken)
    {
        var cred = new DefaultAzureCredential();
        //var cred = new AzureCliCredential();
        var azDoScope = "499b84ac-1321-427f-aa17-267ca6975798";
        var requestContext = new TokenRequestContext([azDoScope]);
        var token = await cred.GetTokenAsync(requestContext, cancellationToken);
        return token;
    }
}