using System.Net;
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
            var httpResponseMessage = response.GetHttpResponseMessage();
            var statusCode = (int)httpResponseMessage.StatusCode;
            return Result.Ok(mapper(response.Value)).AddStatusCode(statusCode);
        }

        return response.ToResult();
    }

    public static Result<TResult> FromSdkExceptionToResult<TResult>(this HttpResponseMessage response)
        where TResult : class
    {
        // In case of timeout
        if (response.StatusCode == HttpStatusCode.RequestTimeout)
            return Result.Fail("Request timed out").Add408RequestTimeoutError().LogError();

        // Weird case where the status code is 200 but the content is "Bad Gateway"
        if (response.IsSuccessStatusCode && response.Content.ToString()!.Contains("Bad Gateway"))
            return Result.Fail("Server responded with Bad Gateway").Add502BadGatewayError();

        return Result.Fail("Request failed").AddStatusCode((int)response.StatusCode).LogDebug();
    }

    private static HttpResponseMessage GetHttpResponseMessage<T>(this T response) =>
        (
            typeof(T).GetProperty(nameof(PostUsersSignInDataResponse.RawResponse))!.GetValue(response)
            as HttpResponseMessage
        )!;
}
