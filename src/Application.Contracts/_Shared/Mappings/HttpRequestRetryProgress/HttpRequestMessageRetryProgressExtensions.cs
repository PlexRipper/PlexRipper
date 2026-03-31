namespace Reaparr.Application.Contracts;

public static class HttpRequestMessageRetryProgressExtensions
{
    private static readonly HttpRequestOptionsKey<Action<HttpRequestRetryProgress>> RetryProgressCallbackKey = new(
        nameof(RetryProgressCallbackKey)
    );

    private static readonly HttpRequestOptionsKey<int> RetryCountKey = new(nameof(RetryCountKey));

    public static void SetRetryProgressCallback(
        this HttpRequestMessage request,
        Action<HttpRequestRetryProgress>? callback
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        if (callback is null)
            return;

        request.Options.Set(RetryProgressCallbackKey, callback);
    }

    public static Action<HttpRequestRetryProgress>? GetRetryProgressCallback(this HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Options.TryGetValue(RetryProgressCallbackKey, out var callback) ? callback : null;
    }

    public static void SetRetryCount(this HttpRequestMessage request, int retryCount)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Options.Set(RetryCountKey, retryCount);
    }

    public static int? GetRetryCount(this HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Options.TryGetValue(RetryCountKey, out int retryCount) ? retryCount : null;
    }
}
