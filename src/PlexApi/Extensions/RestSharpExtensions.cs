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
    /// <typeparam name="T">The parsed type of the response when successful.</typeparam>
    /// <returns>Returns Result.Ok() whether the response was successful or failed, on unhandled exception will return Result.Fail()</returns>
    public static async Task<Result<RestResponse<T>>> SendRequestWithPolly<T>(this RestClient restClient, RestRequest request, int retryCount = 2)
    {
        try
        {
            RestResponse<T> response;
            var policyResult = await Policy
                .HandleResult<RestResponse>(x => !x.IsSuccessful)
                .WaitAndRetryAsync(retryCount, retryAttempt =>
                {
                    var timeToWait = TimeSpan.FromSeconds(retryAttempt * 1);
                    Log.Warning(
                        $"Request: {request.Resource} failed, waiting {timeToWait.TotalSeconds} seconds before retrying again ({retryAttempt} of {retryCount}).");
                    return timeToWait;
                })
                .ExecuteAndCaptureAsync(async () =>
                {
                    response = await restClient.ExecuteAsync<T>(request);
                    return response;
                });

            if (policyResult.Outcome == OutcomeType.Successful)
            {
                return Result.Ok(policyResult.Result as RestResponse<T>);
            }

            return Result.Ok(policyResult.FinalHandledResult as RestResponse<T>);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}