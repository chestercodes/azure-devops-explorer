using OneOf;

namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiResult<TJson> : OneOfBase<TJson, AzureDevopsApiError>
{
    protected AzureDevopsApiResult(OneOf<TJson, AzureDevopsApiError> input) : base(input)
    {
    }

    public static implicit operator AzureDevopsApiResult<TJson>(TJson _) => new AzureDevopsApiResult<TJson>(_);
    public static implicit operator AzureDevopsApiResult<TJson>(AzureDevopsApiError _) => new AzureDevopsApiResult<TJson>(_);

    public static AzureDevopsApiResult<TJson> Ok(TJson input)
    {
        return new AzureDevopsApiResult<TJson>(OneOf<TJson, AzureDevopsApiError>.FromT0(input));
    }
}
