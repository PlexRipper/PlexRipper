using System.Net;
using System.Text.Json;
using LukeHagar.PlexAPI.SDK.Models.Errors;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PlexRipper.PlexApi;

public static class HttpClientExtensions
{
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
            var statusCode = (int)httpResponseMessage.StatusCode;
            return Result.Ok(mapper(response.Value)).AddStatusCode(statusCode);
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

        return Result.Fail("Request failed").AddStatusCode((int)response.StatusCode).WithErrors(errors ?? []);
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
}
