using Newtonsoft.Json;
using OneOf;

namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiError : OneOfBase<(ErrorResponse, Exception), Exception>
{
    protected AzureDevopsApiError(OneOf<(ErrorResponse, Exception), Exception> input) : base(input)
    {
    }

    public static AzureDevopsApiError FromError(ErrorResponse error, Exception ex)
    {
        return new AzureDevopsApiError(OneOf<(ErrorResponse, Exception), Exception>.FromT0((error, ex)));
    }

    public static AzureDevopsApiError FromEx(Exception ex)
    {
        return new AzureDevopsApiError(OneOf<(ErrorResponse, Exception), Exception>.FromT1(ex));
    }

    public string AsError
    {
        get
        {
            var msg = this.Match(a => JsonConvert.SerializeObject(a.Item1) + a.Item2.ToString(), x => x.ToString());
            return msg;
        }
    }
}
