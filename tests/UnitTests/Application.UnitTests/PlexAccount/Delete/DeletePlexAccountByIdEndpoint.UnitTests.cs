using Data.Contracts;

namespace PlexRipper.Application.UnitTests;

public class DeletePlexAccountByIdEndpoint_UnitTests : BaseUnitTest<CreatePlexAccountEndpoint>
{
    public DeletePlexAccountByIdEndpoint_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldDeleteAllRelatedDataWhenAccountIsDeleted()
    {
        // Arrange
        await SetupDatabase(
            1223,
            config =>
            {
                config.PlexAccountCount = 1;
                config.PlexServerCount = 5;
                config.PlexLibraryCount = 3;
                config.MovieCount = 10;
                config.TvShowCount = 10;
            }
        );

        var testAccount = IDbContext.PlexAccounts.IncludeServerAccess().IncludeLibraryAccess().FirstOrDefault();
        testAccount.ShouldNotBeNull();
        testAccount.PlexServers.ShouldNotBeEmpty();
        testAccount.PlexLibraries.ShouldNotBeEmpty();
        IDbContext.PlexMovies.ShouldNotBeEmpty();
        IDbContext.PlexTvShows.ShouldNotBeEmpty();

        // Act
        var ep = SetupEndpointUnitTest<DeletePlexAccountByIdEndpoint>();
        await ep.HandleAsync(new DeletePlexAccountByIdRequest(testAccount.Id), CancellationToken.None);
        var result = ep.Response;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        IDbContext.PlexAccounts.ToList().ShouldBeEmpty();
        IDbContext.PlexServers.ToList().ShouldBeEmpty();
        IDbContext.PlexLibraries.ToList().ShouldBeEmpty();
        IDbContext.PlexMovies.ToList().ShouldBeEmpty();
        IDbContext.PlexTvShows.ToList().ShouldBeEmpty();
    }
}
