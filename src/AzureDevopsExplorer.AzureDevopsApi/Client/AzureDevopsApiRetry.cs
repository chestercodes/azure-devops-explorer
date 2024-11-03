using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Wrap;
using System.Net;

namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiRetry
{
    public static bool IsTransientError(HttpRequestException exception)
    {
        HttpStatusCode[] httpStatusCodesWorthRetrying =
        {
            HttpStatusCode.RequestTimeout, // 408
            HttpStatusCode.BadGateway, // 502
            HttpStatusCode.ServiceUnavailable, // 503
            HttpStatusCode.GatewayTimeout // 504
        };

        return exception.StatusCode.HasValue && httpStatusCodesWorthRetrying.Contains(exception.StatusCode.Value);
    }

    public static readonly Func<ILogger, AsyncRetryPolicy> GetTransientPolicy =
        logger =>
            Policy.Handle<HttpRequestException>(IsTransientError)
           .WaitAndRetryAsync(5, retryAttempt =>
           {
               var nextAttemptIn = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
               logger.LogInformation($"Retry attempt {retryAttempt} to make request. Next try on {nextAttemptIn.TotalSeconds} seconds.");
               return nextAttemptIn;
           });

    private static readonly int MinSecondsToWait = 30;
    private static readonly string MinWaitMessage = $"429 received, cannot parse retry-after header so wait {MinSecondsToWait} seconds";
    private static readonly int MaxSecondsToWait = 1 /* minutes */ * 60;
    private static readonly string RetryAfterLongerThanMaxWaitMessage = $"429 received, expected wait is longer than we want to so we will wait for {MaxSecondsToWait} seconds";

    public static readonly Func<ILogger, AsyncRetryPolicy> GetTooManyRequestsPolicy =
        logger =>
            Policy.Handle<HttpRequestException>(x => x.StatusCode == HttpStatusCode.TooManyRequests)
           .WaitAndRetryAsync(10,
                sleepDurationProvider: (n, ex, cxt) =>
               {
                   var httpEx = ex as HttpRequestException;
                   if (httpEx == null)
                   {
                       throw ex;
                   }

                   var secondsToWait = MinSecondsToWait;
                   var messageToLog = MinWaitMessage;
                   //if (httpEx.HttpRequestError.Response.Headers.TryGetFirst("Retry-After", out string retryAfter))
                   //{
                   //    if (int.TryParse(retryAfter, out int retryAfterInt))
                   //    {
                   //        if (retryAfterInt > MaxSecondsToWait)
                   //        {
                   //            secondsToWait = MaxSecondsToWait;
                   //            messageToLog = RetryAfterLongerThanMaxWaitMessage;
                   //        }
                   //        else
                   //        {
                   //            secondsToWait = retryAfterInt;
                   //            messageToLog = $"429 received, listen to retry-after header and wait {retryAfterInt} seconds";
                   //        }
                   //    }
                   //}

                   logger.LogInformation(messageToLog);
                   return TimeSpan.FromSeconds(secondsToWait);
               },
                onRetryAsync: (ex, timespan, n, cxt) => { return Task.CompletedTask; });

    public static readonly Func<ILogger, AsyncPolicyWrap> GetPolicy =
        logger =>
        {
            var transientPolicy = GetTransientPolicy(logger);
            var tooManyRequestsPolicy = GetTooManyRequestsPolicy(logger);
            return tooManyRequestsPolicy.WrapAsync(transientPolicy);
        };
}