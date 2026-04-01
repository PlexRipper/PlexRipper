using System.Net;
using FluentResults;

namespace Reaparr.FluentResultExtensions;

public static class HttpClientResultExtensions
{
    private const string HTTP_RESPONSE_MESSAGE_METADATA_KEY = "HttpResponseMessage";

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
            return responseResult;

        var response = responseResult.Value;
        if (response.IsSuccessStatusCode)
            return Result.Ok(response);

        var errorMessage = response.ReasonPhrase ?? "Request failed";
        var result = Result.Fail<HttpResponseMessage>(errorMessage).AddStatusCode(response.StatusCode, errorMessage);
        result.Errors[0].Metadata[HTTP_RESPONSE_MESSAGE_METADATA_KEY] = response;
        return result;
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
            ?.Metadata.GetValueOrDefault(HTTP_RESPONSE_MESSAGE_METADATA_KEY);
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

    private static Error CreateSendError(Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
            return (Error)ResultExtensions.TaskIsCancelled(nameof(SendResultAsync)).Errors[0];

        if (exception is TaskCanceledException taskCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
                return (Error)ResultExtensions.TaskIsCancelled(nameof(SendResultAsync)).Errors[0];

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
}
