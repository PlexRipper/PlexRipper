using System.Net;
using System.Text.Json;
using LukeHagar.PlexAPI.SDK.Models.Errors;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Newtonsoft.Json;
using Reaparr.FluentResultExtensions;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Reaparr.PlexApi;

public static class HttpClientExtensions
{
    private const string HttpResponseMessageMetadataKey = "HttpResponseMessage";
    private static readonly ILogger _log = Log.ForContext(typeof(HttpClientExtensions));

    public static async Task<Result<HttpResponseMessage>> SendResultAsync(
        this HttpClient httpClient,
        HttpRequestMessage request,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default
    )
    {
        var responseResult = await Result.Try(
            () => httpClient.SendAsync(request, completionOption, cancellationToken),
            exception => CreateSendError(exception, cancellationToken)
        );

        if (responseResult.IsFailed)
        {
            if (cancellationToken.IsCancellationRequested)
                responseResult.ToResult().LogWarning();
            else
                responseResult.ToResult().LogError();

            return responseResult;
        }

        var response = responseResult.Value;
        if (response.IsSuccessStatusCode)
            return Result.Ok(response);

        var errorMessage = response.ReasonPhrase ?? "Request failed";
        var result = Result.Fail<HttpResponseMessage>(errorMessage).AddStatusCode(response.StatusCode, errorMessage);
        result.Errors[0].Metadata[HttpResponseMessageMetadataKey] = response;
        result.ToResult().LogWarning();
        return result;
    }

    private static Error CreateSendError(Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
            return (Error)ResultExtensions.TaskIsCancelled(nameof(SendResultAsync)).Errors[0];

        if (exception is TaskCanceledException taskCanceledException)
        {
            return new ExceptionalError(taskCanceledException)
                .WithMetadata(ResultExtensions.StatusCodeName, HttpCodes.Status408RequestTimeout)
                .WithMetadata(ResultExtensions.ErrorMessageName, "Request Timeout");
        }

        if (exception is HttpRequestException httpRequestException)
        {
            return new ExceptionalError(httpRequestException)
                .WithMetadata(ResultExtensions.StatusCodeName, HttpCodes.Status502BadGateway)
                .WithMetadata(ResultExtensions.ErrorMessageName, "Network error");
        }

        return new ExceptionalError(exception)
            .WithMetadata(ResultExtensions.StatusCodeName, HttpCodes.Status500InternalServerError)
            .WithMetadata(ResultExtensions.ErrorMessageName, exception.Message);
    }

    /// <summary>
    /// This will convert from SpeakEasy exceptions to the use of FluentResults
    /// </summary>
    /// <param name="operation"> The SpeakEasy Plex SDK endpoint method to convert the result for </param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<Result<T>> ToResponse<T>(this Task<T> operation)
        where T : class
    {
        try
        {
            return Result.Ok(await operation);
        }
        catch (SDKException e)
        {
            return e.RawResponse.FromSdkExceptionToResult<T>();
        }
        catch (JsonSerializationException e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
        catch (ResponseValidationException e)
        {
            _log.Here().Error("Failed response validation: {Body}", e.Body);
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
        catch (Exception e)
        {
            var errorsProperty = e.GetType().GetProperty("Errors");
            var rawResponseProperty = e.GetType().GetProperty("RawResponse");

            if (errorsProperty != null && rawResponseProperty != null)
            {
                var rawResponse = rawResponseProperty.GetValue(e);
                if (rawResponse is null)
                    return Result.Fail(new ExceptionalError(e)).LogError();

                var errors = errorsProperty.GetValue(e);
                var parsedErrors = JsonSerializer.Deserialize<List<PlexError>>(JsonSerializer.Serialize(errors));

                return ((HttpResponseMessage)rawResponse).FromSdkExceptionToResult<T>(parsedErrors);
            }

            if (rawResponseProperty != null)
            {
                var rawResponse = rawResponseProperty.GetValue(e);
                if (rawResponse is null)
                    return Result.Fail(new ExceptionalError(e)).LogError();

                return ((HttpResponseMessage)rawResponse).FromSdkExceptionToResult<T>();
            }

            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public static Result<TResult> ToApiResult<TResponse, TResult>(
        this Result<TResponse> response,
        Func<TResponse, TResult> mapper
    )
        where TResponse : class
    {
        if (response.IsSuccess)
        {
            var httpResponseMessage = response.Value.GetHttpResponseMessage();
            return Result.Ok(mapper(response.Value)).AddStatusCode(httpResponseMessage.StatusCode);
        }

        // TODO Might need to add more error handling here such as AddStatusCode
        return response.ToResult();
    }

    public static Result<TResult> FromSdkExceptionToResult<TResult>(
        this HttpResponseMessage response,
        List<PlexError>? errors = null
    )
        where TResult : class
    {
        switch (response.StatusCode)
        {
            // In case of unauthorized
            case HttpStatusCode.Unauthorized:
                return Result.Fail("Unauthorized").AddPlex401UnauthorizedError().WithErrors(errors ?? []);

            // In case of timeout
            case HttpStatusCode.RequestTimeout:
                return Result.Fail("Request timed out").Add408RequestTimeoutError().WithErrors(errors ?? []);
        }

        // Weird case where the status code is 200 but the content is "Bad Gateway"
        if (response.IsSuccessStatusCode && response.Content.ToString()!.Contains("Bad Gateway"))
            return Result.Fail("Server responded with Bad Gateway").Add502BadGatewayError().WithErrors(errors ?? []);

        return Result.Fail("Request failed").AddStatusCode(response.StatusCode).WithErrors(errors ?? []);
    }

    private static HttpResponseMessage GetHttpResponseMessage<T>(this T response) =>
        (
            typeof(T).GetProperty(nameof(PostUsersSignInDataResponse.RawResponse))!.GetValue(response)
            as HttpResponseMessage
        )!;

    public static async Task<string> ReadAsFormattedJsonAsync(this HttpContent? content)
    {
        if (content == null)
            return "HttpContent is null.";

        var contentType = content.Headers.ContentType?.MediaType;
        var contentLength = content.Headers.ContentLength;

        // Check if the content indicates a file download via Content-Disposition header.
        if (
            content.Headers.ContentDisposition != null
            && content.Headers.ContentDisposition.DispositionType.Equals(
                "attachment",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return $"Content is a file download. Filename: {content.Headers.ContentDisposition.FileName}";
        }

        // Only process JSON content types
        if (contentType?.Contains("json", StringComparison.OrdinalIgnoreCase) != true)
        {
            var sizeInfo = contentLength.HasValue ? $"{contentLength.Value:N0} bytes" : "unknown size";
            return $"Non-JSON content ({contentType ?? "unknown"}, {sizeInfo}) - not formatting as JSON.";
        }

        // Check content length to avoid buffer overflow for large JSON responses
        const long maxReadContentSize = 1024 * 1024; // 1MB limit for reading JSON content as string

        if (contentLength.HasValue && contentLength.Value > maxReadContentSize)
        {
            var sizeInfo = $"{contentLength.Value:N0} bytes";
            return $"Large JSON content ({sizeInfo}) - not reading to avoid buffer overflow.";
        }

        try
        {
            var stringResponse = await content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(stringResponse))
                return "Content is empty.";

            using var doc = JsonDocument.Parse(stringResponse);
            return JsonSerializer.Serialize(doc, DefaultJsonSerializerOptions.UserSettingsOptions);
        }
        catch (JsonException)
        {
            return "Content is not valid JSON.";
        }
    }

    public static HttpResponseMessage ToHttpResponseMessage(
        this Result<HttpResponseMessage> responseResult,
        HttpRequestMessage request
    )
    {
        if (responseResult.IsSuccess)
            return responseResult.Value;

        var originalResponse = responseResult
            .Errors.FirstOrDefault()
            ?.Metadata.GetValueOrDefault(HttpResponseMessageMetadataKey);
        if (originalResponse is HttpResponseMessage httpResponseMessage)
            return httpResponseMessage;

        var statusCode = responseResult.ToResult().FindStatusCode();
        var message = responseResult.Errors.FirstOrDefault()?.Message ?? "Request failed";
        var reasonPhrase = statusCode switch
        {
            408 => "Request Timeout",
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            _ => message,
        };

        return new HttpResponseMessage((HttpStatusCode)(statusCode == 0 ? 500 : statusCode))
        {
            Content = new StringContent(message),
            ReasonPhrase = reasonPhrase,
            RequestMessage = request,
        };
    }
}
