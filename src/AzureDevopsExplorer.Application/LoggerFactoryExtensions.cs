using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application;

public static class LoggerFactoryExtensions
{
    public static ILogger Create<T>(this ILoggerFactory loggerFactory, T callingClassObject)
    {
        var t = callingClassObject.GetType().Name;
        return loggerFactory.CreateLogger(t);
    }
}
