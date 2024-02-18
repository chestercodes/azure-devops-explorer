namespace AzureDevopsExplorer.AzureDevopsApi.Auth;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.OAuth;
using System.Threading;

public class AzureAdoTokenProvider(IssuedTokenCredential credential, Uri serverUrl)
    : IssuedTokenProvider(credential, serverUrl, serverUrl)
{
    public override bool GetTokenIsInteractive => false;

    protected override async Task<IssuedToken> OnGetTokenAsync(IssuedToken failedToken, CancellationToken cancellationToken)
    {
        var tp = new AzureDevopsAccessTokenProvider();
        var token = await tp.GetAccessToken(cancellationToken);
        return new VssOAuthAccessToken(token.Token, token.ExpiresOn.UtcDateTime);
    }
}
