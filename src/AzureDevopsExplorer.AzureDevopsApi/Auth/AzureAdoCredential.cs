namespace AzureDevopsExplorer.AzureDevopsApi.Auth;
using Microsoft.VisualStudio.Services.Common;

public class AzureAdoCredential() : FederatedCredential(null)
{
    protected override IssuedTokenProvider OnCreateTokenProvider(Uri serverUrl, IHttpResponse response) =>
        new AzureAdoTokenProvider(this, serverUrl);

    public override VssCredentialsType CredentialType => VssCredentialsType.OAuth;
}
