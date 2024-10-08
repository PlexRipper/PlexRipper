using System.Net;
using System.Text.Json;
using Application.Contracts;
using Logging.Interface;
using PlexApi.Contracts;
using Polly;
using RestSharp;

namespace PlexRipper.PlexApi;

public static class RestSharpExtensions
{
    #region Properties

    public static JsonSerializerOptions SerializerOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new LongToDateTime() },
    };

    #endregion

    #region Methods

    #region Public

    /// <summary>
    /// Sends a <see cref="RestRequest"/> with a <see cref="Policy"/>
    /// </summary>
    /// <param name="restClient">The <see cref="RestClient"/> to use for the request.</param>
    /// <param name="request">The <see cref="RestRequest"/> to send.</param>
    /// <param name="retryCount">How many times should the request be attempted before giving up.</param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T">The parsed type of the response when successful.</typeparam>
    /// <returns>Returns Result.Ok() whether the response was successful or failed, on unhandled exception will return Result.Fail()</returns>
    public static async Task<RestResponse<T>> SendRequestWithPolly<T>(
        this RestClient restClient,
        RestRequest request,
        int retryCount = 2,
        Action<PlexApiClientProgress>? action = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        RestResponse<T>? response = null;
        var retryIndex = 0;
        var policyResult = Policy
            .HandleResult<RestResponse>(x => !x.IsSuccessful)
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt =>
                {
                    var timeToWait = TimeSpan.FromSeconds(retryAttempt * 1);
                    _log.Warning(
                        "Request to: {Url} failed, waiting {TotalSeconds} seconds before retrying again ({RetryAttempt} of {RetryCount})",
                        request.Resource,
                        timeToWait.TotalSeconds,
                        retryAttempt,
                        retryCount
                    );

                    retryIndex = retryAttempt;

                    // Send update
                    action?.Send(response, retryAttempt, retryCount, (int)timeToWait.TotalSeconds);

                    return timeToWait;
                }
            );

        var responseResult = await policyResult.ExecuteAndCaptureAsync(async () =>
        {
            // We store the response here so we have access to the last response in the WaitAndRetryAsync() above.
            response = await restClient.ExecuteAsync<T>(request, cancellationToken);
            return response;
        });

        // Send final progress update
        action?.Send(response, retryIndex, retryCount);

        return ToResponse<T>(responseResult);
    }

    /// <summary>
    /// Generates a <see cref="Result{T}"/> from the <see cref="RestResponse{T}"/>
    /// </summary>
    /// <param name="response"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Result<T> ToResponseResult<T>(this RestResponse<T> response)
        where T : class
    {
        // In case of timeout
        if (response.ResponseStatus == ResponseStatus.TimedOut)
            return Result.Fail("Request timed out").Add408RequestTimeoutError().LogError();

        // Weird case where the status code is 200 but the content is "Bad Gateway"
        if (response.IsSuccessStatusCode && response.Content == "Bad Gateway")
            return Result.Fail("Server responded with Bad Gateway").Add502BadGatewayError();

        // Successful response
        if (response.IsSuccessStatusCode)
            return Result.Ok(response.Data).AddStatusCode((int)response.StatusCode).LogDebug();

        var statusDescription = "";
        if (response.StatusDescription != null)
        {
            statusDescription += $"Response: {response.StatusDescription}";
            if (response.ErrorMessage != null)
                statusDescription += " - ";
        }

        if (response.ErrorMessage != null)
            statusDescription += $"ErrorMessage: {response.ErrorMessage}";

        _log.ErrorLine(statusDescription);

        return ParsePlexErrors(response);
    }

    #endregion

    #region Private

    private static RestResponse<T> ToResponse<T>(PolicyResult<RestResponse> response)
    {
        if (response.Outcome == OutcomeType.Successful)
        {
            if (response.Result.Content != string.Empty)
                _log.Verbose("Response Content: {@Content}", response.Result.Content);
            else
                _log.VerboseLine("Response was empty");

            return response.Result as RestResponse<T>;
        }

        return response.FinalHandledResult as RestResponse<T>;
    }

    /// <summary>
    /// Parses the Plex errors and returns a <see cref="Result{T}"/>
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    private static Result ParsePlexErrors(RestResponse response)
    {
        var requestUrl = response.Request.Resource;
        var statusCode = (int)response.StatusCode;
        var errorMessage = response.ErrorMessage;

        var result = Result.Fail($"Request to {requestUrl} failed with status code: {statusCode} - {errorMessage}");
        try
        {
            var content = response.Content;
            if (response.StatusCode == HttpStatusCode.GatewayTimeout || content.Contains("504 Gateway Time-out"))
            {
                result.Add504GatewayTimeoutError();
                return result;
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage) && response.ErrorMessage.Contains("Timeout"))
                result.Add408RequestTimeoutError(response.ErrorMessage);
            else
                result.AddStatusCode(statusCode, errorMessage);

            if (!string.IsNullOrEmpty(content))
            {
                var errorsResponse =
                    JsonSerializer.Deserialize<PlexErrorsResponseDTO>(content, SerializerOptions)
                    ?? new PlexErrorsResponseDTO();
                if (errorsResponse.Errors != null && errorsResponse.Errors.Any())
                    result.WithErrors(errorsResponse.ToResultErrors());
            }
        }
        catch (Exception)
        {
            return result.WithError(new Error($"Failed to deserialize: {response}"));
        }

        return result.LogError();
    }

    private static void Send(
        this Action<PlexApiClientProgress>? action,
        RestResponse response,
        int retryAttempt,
        int retryCount,
        int timeToWaitSeconds = 0
    )
    {
        var request = response.Request;
        var msg = "Request successful!";

        if (!response.IsSuccessful && response.ResponseStatus != ResponseStatus.TimedOut)
        {
            msg =
                $"Request to: {request.Resource} failed, waiting {timeToWaitSeconds} seconds before retrying again ({retryAttempt} of {retryCount})";
        }

        if (response is { IsSuccessful: false, ResponseStatus: ResponseStatus.TimedOut })
        {
            msg =
                $"Request to: {request.Resource} timed-out, waiting {timeToWaitSeconds} seconds before retrying again ({retryAttempt} of {retryCount})";
        }

        action?.Invoke(
            new PlexApiClientProgress
            {
                StatusCode = (int)response.StatusCode,
                Message = msg,
                RetryAttemptIndex = retryAttempt,
                RetryAttemptCount = retryCount,
                TimeToNextRetry = timeToWaitSeconds,
                Completed = response.ResponseStatus == ResponseStatus.Completed || retryAttempt == retryCount,
                ErrorMessage = response.ErrorMessage ?? "",
                ConnectionSuccessful = response.IsSuccessful,
            }
        );
    }

    #endregion

    #endregion

    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(RestSharpExtensions));
}