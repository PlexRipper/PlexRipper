namespace Reaparr.Application.UnitTests;

public class GetMovieMediaComparisonDetailsCommandHandlerUnitTests
    : BaseUnitTest<GetMovieMediaComparisonDetailsCommandHandler>
{
    [Test]
    public async Task ShouldReturnMissingMovieRow_WhenRemoteMovieHasCurrentOwnedScopeWithoutComparisonHits()
    {
        // Arrange
        await SetupDatabase(
            63501,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.MovieCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, new DateTime(2026, 8, 1, 13, 0, 0, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        ownedLibrary = await GetLibraryAsync(ownedLibrary.Id);
        var remoteMovie = await GetLibraryMovieAsync(remoteLibrary.Id);

        await AddCurrentScopeAsync(remoteLibrary, ownedLibrary, PlexMediaType.Movie);
        await dbContext.SaveChangesNewAsync(CancellationToken);

        var command = new GetMovieMediaComparisonDetailsCommand(remoteMovie.Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        result.Value.PlexMediaId.ShouldBe(remoteMovie.Id);
        result.Value.Type.ShouldBe(PlexMediaType.Movie);
        result.Value.State.ShouldBe(PlexMediaComparisonState.Partial);
        result.Value.Rows.Count.ShouldBe(1);
        var row = result.Value.Rows.Single();
        row.PlexMediaId.ShouldBe(remoteMovie.Id);
        row.Type.ShouldBe(PlexMediaType.Movie);
        row.State.ShouldBe(PlexMediaComparisonState.Missing);
        row.PlexLibraryId.ShouldBe(remoteLibrary.Id);
        row.PlexServerId.ShouldBe(remoteLibrary.PlexServerId);
        row.Children.ShouldBeEmpty();
    }

    private async Task SetOwnedOverrideAsync(int plexServerId, bool ownedOverride)
    {
        await IDbContext.PlexServers
            .Where(x => x.Id == plexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, ownedOverride), CancellationToken);
    }

    private async Task SetLibraryUpdatedAtAsync(int plexLibraryId, DateTime updatedAt)
    {
        await IDbContext.PlexLibraries
            .Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.UpdatedAt, updatedAt), CancellationToken);
    }

    private async Task<PlexLibrary> GetLibraryAsync(int plexLibraryId) =>
        await IDbContext.PlexLibraries
            .Where(x => x.Id == plexLibraryId)
            .SingleAsync(CancellationToken);

    private async Task<PlexMovie> GetLibraryMovieAsync(int plexLibraryId) =>
        await IDbContext.PlexMovies
            .Where(x => x.PlexLibraryId == plexLibraryId)
            .SingleAsync(CancellationToken);

    private async Task AddCurrentScopeAsync(PlexLibrary remoteLibrary, PlexLibrary ownedLibrary, PlexMediaType mediaType)
    {
        var remoteUpdatedAt = await IDbContext.PlexLibraries
            .Where(x => x.Id == remoteLibrary.Id)
            .Select(x => x.UpdatedAt)
            .SingleAsync(CancellationToken);
        var ownedUpdatedAt = await IDbContext.PlexLibraries
            .Where(x => x.Id == ownedLibrary.Id)
            .Select(x => x.UpdatedAt)
            .SingleAsync(CancellationToken);

        var dbContext = IDbContext;
        dbContext.PlexComparisonScopes.Add(new PlexComparisonState
        {
            Id = 0,
            RemotePlexLibraryId = remoteLibrary.Id,
            OwnedPlexLibraryId = ownedLibrary.Id,
            MediaType = mediaType,
            CompletedAt = DateTime.UtcNow,
            RemoteLibraryUpdatedAt = remoteUpdatedAt,
            OwnedLibraryUpdatedAt = ownedUpdatedAt,
        });
        await dbContext.SaveChangesNewAsync(CancellationToken);
    }
}