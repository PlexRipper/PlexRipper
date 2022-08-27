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
    public static async Task<Result<T>> SendRequestWithPolly<T>(
        this RestClient restClient,
        RestRequest request,
        int retryCount = 2,
        Action<PlexApiClientProgress> action = null) where T : class
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

            return GenerateResponseResult<T>(policyResult, action);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }


    private static Result<T> GenerateResponseResult<T>(PolicyResult<RestResponse> response, Action<PlexApiClientProgress> action = null) where T : class
    {
        RestResponse<T> responseResult = null;
        var isSuccessful = response.Outcome == OutcomeType.Successful;
        if (isSuccessful)
            responseResult = response.Result as RestResponse<T>;
        else
            responseResult = response.FinalHandledResult as RestResponse<T>;

        var statusCode = (int)responseResult.StatusCode;
        var statusDescription = responseResult.StatusDescription;
        var requestUrl = responseResult.Request.Resource;
        var errorMessage = responseResult.Content;

        if (action is not null)
        {
            action(new PlexApiClientProgress
            {
                StatusCode = statusCode,
                Message = isSuccessful ? "Request successful!" : errorMessage,
                ConnectionSuccessful = isSuccessful,
                Completed = true,
            });
        }

        if (isSuccessful)
            return Result.Ok(responseResult.Data).LogDebug();

        return Result
            .Fail($"Request to {requestUrl} failed with status code: {statusCode} - {statusDescription}")
            .AddStatusCode(statusCode, statusDescription)
            .WithError(errorMessage)
            .LogError();
    }
}