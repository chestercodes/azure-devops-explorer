using System.Text;

namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public record AzureDevopsPat(string Value);

public interface IGetAuthHeader
{
    string Get();
}

public class AzureDevopsApiAuthHeaderRetriever : IGetAuthHeader
{
    private readonly AzureDevopsPat? pat;
    private readonly CancellationToken cancellationToken;

    public AzureDevopsApiAuthHeaderRetriever(AzureDevopsPat? pat, CancellationToken cancellationToken)
    {
        this.pat = pat;
        this.cancellationToken = cancellationToken;
    }

    public string Get()
    {
        if (pat != null)
        {
            var asBase64 = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", pat.Value)));
            return $"Basic {asBase64}";
        }
        return $"Bearer {GetBearerToken()}";
    }

    private string GetBearerToken()
    {
        var tokenProvider = new AzureDevopsAccessTokenProvider();
        var accessToken = tokenProvider.GetAccessToken(cancellationToken).Result;
        var bearerToken = accessToken.Token;
        return bearerToken;
    }
}
