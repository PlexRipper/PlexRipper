using LukeHagar.PlexAPI.SDK.Utils;
using WireMock.ResponseBuilders;

namespace PlexRipper.BaseTests;

public static class WireMockExtensions
{
    public static IResponseBuilder WithPlexSdkJsonContent(this IResponseBuilder builder, object content) =>
        builder.WithBody(Utilities.SerializeJSON(content, "string"));
}
