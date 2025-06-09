using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;

namespace IntegrationTests;

public class RefreshLibraryMediaEndpointIntegrationTests : BaseIntegrationTests
{
    public RefreshLibraryMediaEndpointIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldFullyRefreshLibraryMedia_WhenPlexLibraryIsOfTypeMovieAndCommandIsSent()
    {
        // Arrange
        var seed = new Seed(8932);
        const int serverCount = 1;
        const int libraryCount = 3;
        const int movieCount = 500;
        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.HttpClientOptions = (x, _) =>
                {
                    x.SetupIdentityRequest(seed);
                };

                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexMovieLibraryCount = 1;
                    x.MovieCount = 0;
                };

                config.BaseMockHttpClientOptions = x =>
                {
                    x.GenerateFromDatabase = true;
                    x.PlexServerAccessCount = serverCount;
                    x.MovieLibraryCount = libraryCount;
                    x.MoviesPerLibraryCount = movieCount;
                };
            }
        );

        var plexLibrary = await container.DbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Act
        var client = container.GetApiClient();
        await client.SignIn();

        var testResult = await client.GETAsync<
            RefreshLibraryMediaEndpoint,
            RefreshLibraryMediaEndpointRequest,
            ResultDTO<PlexLibraryDTO>
        >(new RefreshLibraryMediaEndpointRequest(plexLibrary.Id));

        // Assert
        var result = testResult.Result;
        result.IsSuccess.ShouldBeTrue();
        testResult.Response.IsSuccessStatusCode.ShouldBeTrue();

        // Verify the library was refreshed
        var refreshedLibrary = await container
            .DbContext.PlexLibraries.Include(x => x.Movies)
            .FirstOrDefaultAsync(x => x.Id == plexLibrary.Id);

        refreshedLibrary.ShouldNotBeNull();
        refreshedLibrary.Movies.Count.ShouldBe(movieCount);
        refreshedLibrary.SyncedAt.ShouldNotBeNull();
    }
}
