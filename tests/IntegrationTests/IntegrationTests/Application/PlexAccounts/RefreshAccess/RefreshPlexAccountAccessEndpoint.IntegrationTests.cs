using System.Net;
using Application.Contracts;
using FastEndpoints;
using FluentResultExtensions;
using Moq.Contrib.HttpClient;
using PlexRipper.Application;

namespace IntegrationTests.PlexAccounts.RefreshAccess;

[Collection("Sequential")]
public class RefreshPlexAccountAccessEndpointIntegrationTestsIntegrationTests : BaseIntegrationTests
{
    public RefreshPlexAccountAccessEndpointIntegrationTestsIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldRefreshAccessPlexAccountAndNot500_WhenAPlexAccountIsUnauthorized()
    {
        // Arrange
        var plexAccountCount = 2;
        var plexServerCount = 5;
        var plexLibraryCount = 5;
        var seed = new Seed(22453);
        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = plexAccountCount;
                    x.PlexServerCount = plexServerCount;
                    x.PlexLibraryCount = plexLibraryCount;
                };
                config.BaseMockHttpClientOptions = x =>
                {
                    x.PlexServerAccessCount = plexServerCount;
                    x.MovieLibraryCount = plexLibraryCount;
                    x.MoviesPerLibraryCount = 500;
                    x.SetServerResourcesResponse = HttpStatusCode.Unauthorized;
                };
            }
        );

        // Act
        var client = container.GetApiClient();
        await client.SignIn();

        var response = await client.GETAsync<
            RefreshPlexAccountAccessEndpoint,
            RefreshPlexAccountAccessEndpointRequest,
            ResultDTO<List<RefreshPlexAccountAccessRapportDTO>>
        >(new RefreshPlexAccountAccessEndpointRequest());

        // Assert
        var result = response.Result;
        result.StatusCode.ShouldBe(200);
        var rapports = result.Value.ShouldNotBeNull();
        rapports.Count.ShouldBe(plexAccountCount);

        foreach (var rapport in rapports)
        {
            rapport.Access.Count.ShouldBe(plexServerCount);
            foreach (var serverAccess in rapport.Access)
            {
                serverAccess.State.ShouldBe(PlexAccessState.Revoked);
                serverAccess.IsServerOffline.ShouldBeFalse();
                serverAccess.LibraryAccess.Count.ShouldBe(plexLibraryCount);

                foreach (var libraryAccess in serverAccess.LibraryAccess)
                {
                    libraryAccess.State.ShouldBe(PlexAccessState.Revoked);
                    libraryAccess.PlexServerId.ShouldBe(serverAccess.PlexServerId);
                }
            }
        }

        container.DbContext.PlexAccountServers.ToList().Count.ShouldBe(0);
        container.DbContext.PlexAccountLibraries.ToList().Count.ShouldBe(0);
    }
}
