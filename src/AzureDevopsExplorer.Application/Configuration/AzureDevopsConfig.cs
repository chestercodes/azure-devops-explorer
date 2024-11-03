using AzureDevopsExplorer.AzureDevopsApi.Client;

namespace AzureDevopsExplorer.Application.Configuration;

public class AzureDevopsConfig
{
    public required string Organisation { get; set; }
    public List<string>? Projects { get; set; }
    public string? Pat { get; set; }
}

public static class StringExtensions
{
    public static AzureDevopsPat? LiftToPat(this string? value)
    {
        if (value == null)
        {
            return null;
        }

        return new AzureDevopsPat(value);
    }
}