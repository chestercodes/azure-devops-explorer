using System.Net;

namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiError
{
    public HttpRequestException Exception { get; private set; }

    public AzureDevopsApiError(HttpRequestException exception)
    {
        Exception = exception;
    }

    private HashSet<HttpStatusCode> httpStatusCodesWorthRetrying =
        new HashSet<HttpStatusCode> {
            HttpStatusCode.RequestTimeout, // 408
            HttpStatusCode.BadGateway, // 502
            HttpStatusCode.ServiceUnavailable, // 503
            HttpStatusCode.GatewayTimeout // 504
        };

    public bool ProbablyNotTransientError
    {
        get
        {
            if (Exception.StatusCode == null)
            {
                return true;
            }
            var isWorthRetrying = httpStatusCodesWorthRetrying.Contains(Exception.StatusCode.Value);
            return isWorthRetrying == false;
        }
    }

    public string AsError
    {
        get
        {
            return this.ToString();
        }
    }
}
