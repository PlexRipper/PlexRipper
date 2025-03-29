using System.Net;
using System.Text.Json;
using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace PlexRipper.PlexApi;

public static class HttpClientExtensions
{
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

    public static async Task<string> ReadAsFormattedJsonAsync(this HttpContent content)
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        var stringResponse = await content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(stringResponse);
        return JsonSerializer.Serialize(doc, DefaultJsonSerializerOptions.UserSettingsOptions);
    }
}
