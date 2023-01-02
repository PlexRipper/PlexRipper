using JustEat.HttpClientInterception;
using PlexRipper.PlexApi.Common;

namespace PlexRipper.BaseTests;

public static class MockPlexApiRequests
{
    public static HttpRequestInterceptionBuilder SetupSignIn(this HttpRequestInterceptionBuilder builder, MockPlexApiConfig config)
    {
        var query = builder
            .ForPost()
            .ForPath(PlexApiPaths.SignInPath)
            .IgnoringQuery();

        if (config.SignInResponseIsValid)
        {
            query.Responds()
                .WithStatus(201) // 201 is correct here, Plex for some reason gives this back on this request
                .WithJsonContent(FakePlexApiData.GetPlexSignInResponse().Generate());
        }
        else
        {
            query.Responds()
                .WithStatus(401)
                .WithJsonContent(FakePlexApiData.GetFailedPlexSignInResponse());
        }

        return query;
    }

    public static HttpRequestInterceptionBuilder SetupServerResources(this HttpRequestInterceptionBuilder builder, MockPlexApiConfig config)
    {
        var query = builder
            .ForGet()
            .ForPath(PlexApiPaths.ServerResourcesPath)
            .IgnoringQuery();

        if (!config.UnauthorizedAccessiblePlexServers)
        {
            query.Responds()
                .WithStatus(200)
                .WithJsonContent(FakePlexApiData
                    .GetServerResource()
                    .Generate(config.AccessiblePlexServers));
        }
        else
        {
            query.Responds()
                .WithStatus(401)
                .WithJsonContent(FakePlexApiData.GetFailedServerResourceResponse());
        }

        return query;
    }
}