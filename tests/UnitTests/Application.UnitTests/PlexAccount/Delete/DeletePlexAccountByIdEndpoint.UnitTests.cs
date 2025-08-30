using Reaparr.BaseTests;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class DeletePlexAccountByIdEndpointUnitTests : BaseUnitTest
{
    public DeletePlexAccountByIdEndpointUnitTests(ITestOutputHelper output)
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
                config.PlexMovieLibraryCount = 3;
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

        mock.SendRefreshNotification();

        // Act
        var ep = SetupEndpointUnitTest<DeletePlexAccountByIdEndpoint>();
        await ep.HandleAsync(new DeletePlexAccountByIdRequest(testAccount.Id), CancellationToken);
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
