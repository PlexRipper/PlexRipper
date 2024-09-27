using JustEat.HttpClientInterception;
using LukeHagar.PlexAPI.SDK.Utils;

namespace PlexRipper.BaseTests;

public static class HttpClientInterceptorExtensions
{
    public static HttpRequestInterceptionBuilder WithPlexSdkJsonContent(
        this HttpRequestInterceptionBuilder builder,
        object content
    ) => builder.WithContent(Utilities.SerializeJSON(content, "string"));
}
