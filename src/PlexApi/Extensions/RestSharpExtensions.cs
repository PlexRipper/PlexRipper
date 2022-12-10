using PlexRipper.Application;
using Polly;
using RestSharp;

namespace PlexRipper.PlexApi.Extensions;

public static class RestSharpExtensions
{
    /// <summary>
    /// Sends a <see cref="RestRequest"/> with a <see cref="Policy"/>
    /// </summary>
    /// <param name="restClient">The <see cref="RestClient"/> to use for the request.</param>
    /// <param name="request">The <see cref="RestRequest"/> to send.</param>
    /// <param name="retryCount">How many times should the request be attempted before giving up.</param>
    /// <param name="action"></param>
    /// <typeparam name="T">The parsed type of the response when successful.</typeparam>
    /// <returns>Returns Result.Ok() whether the response was successful or failed, on unhandled exception will return Result.Fail()</returns>
    public static async Task<RestResponse<T>> SendRequestWithPolly<T>(
        this RestClient restClient,
        RestRequest request,
        int retryCount = 2,
        Action<PlexApiClientProgress> action = null) where T : class
    {
        RestResponse<T> response = null;
        var policyResult = await Policy
            .HandleResult<RestResponse>(x => !x.IsSuccessful)
            .WaitAndRetryAsync(retryCount, retryAttempt =>
            {
                var timeToWait = TimeSpan.FromSeconds(retryAttempt * 1);
                var msg =
                    $"Request: {request.Resource} failed, waiting {timeToWait.TotalSeconds} seconds before retrying again ({retryAttempt} of {retryCount}).";
                Log.Warning(msg);
                if (action is not null)
                {
                    action(new PlexApiClientProgress
                    {
                        StatusCode = (int)response.StatusCode,
                        Message = msg,
                        RetryAttemptIndex = retryAttempt,
                        RetryAttemptCount = retryCount,
                        TimeToNextRetry = (int)timeToWait.TotalSeconds,
                        Completed = false,
                    });
                }

                return timeToWait;
            })
            .ExecuteAndCaptureAsync(async () =>
            {
                // We store the response here so we have access to the last response in the WaitAndRetryAsync() above.
                response = await restClient.ExecuteAsync<T>(request);
                return response;
            });

        return ToResponse<T>(policyResult);
    }


    private static RestResponse<T> ToResponse<T>(PolicyResult<RestResponse> response)
    {
        var isSuccessful = response.Outcome == OutcomeType.Successful;
        if (isSuccessful)
            return response.Result as RestResponse<T>;

        return response.FinalHandledResult as RestResponse<T>;
    }
}