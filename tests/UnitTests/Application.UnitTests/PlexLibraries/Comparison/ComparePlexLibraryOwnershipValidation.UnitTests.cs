namespace Reaparr.Application.UnitTests;

public class CompareMoviePlexLibraryCommandOwnershipUnitTests : BaseUnitTest<CompareMoviePlexLibraryCommandHandler>
{
    [Test]
    public async Task ShouldTreatOwnedOverrideFalseAsRemote_WhenAccountLibraryStillOwned()
    {
        // Arrange
        await SetupDatabase(
            32,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        // Act
        var result = await Sut.ExecuteAsync(
            new CompareMoviePlexLibraryCommand(remoteLibrary.Id, ownedLibrary.Id),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldUpdateExistingComparisonScopeSnapshots_WhenScopeAlreadyExists()
    {
        // Arrange
        await SetupDatabase(33, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        var remoteUpdatedAt = new DateTime(2026, 7, 21, 17, 24, 15, DateTimeKind.Utc);
        var ownedUpdatedAt = new DateTime(2026, 7, 21, 14, 8, 33, DateTimeKind.Utc);
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, remoteUpdatedAt);
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, ownedUpdatedAt);
        dbContext.PlexComparisonScopes.Add(new PlexComparisonState
        {
            Id = 0,
            RemotePlexLibraryId = remoteLibrary.Id,
            OwnedPlexLibraryId = ownedLibrary.Id,
            MediaType = PlexMediaType.Movie,
            CompletedAt = new DateTime(2026, 7, 20, 22, 19, 16, DateTimeKind.Utc),
            RemoteLibraryUpdatedAt = new DateTime(2026, 7, 20, 14, 7, 34, DateTimeKind.Utc),
            OwnedLibraryUpdatedAt = new DateTime(2026, 7, 20, 20, 13, 32, DateTimeKind.Utc),
        });
        await dbContext.SaveChangesNewAsync(CancellationToken);

        // Act
        var result = await Sut.ExecuteAsync(
            new CompareMoviePlexLibraryCommand(remoteLibrary.Id, ownedLibrary.Id),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var scope = await IDbContext.PlexComparisonScopes.SingleAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id
                 && x.OwnedPlexLibraryId == ownedLibrary.Id
                 && x.MediaType == PlexMediaType.Movie,
            CancellationToken
        );
        scope.RemoteLibraryUpdatedAt.ShouldBe(remoteUpdatedAt);
        scope.OwnedLibraryUpdatedAt.ShouldBe(ownedUpdatedAt);
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
}

public class CompareTvShowPlexLibraryCommandOwnershipUnitTests : BaseUnitTest<CompareTvShowPlexLibraryCommandHandler>
{
    [Test]
    public async Task ShouldTreatOwnedOverrideFalseAsRemote_WhenAccountLibraryStillOwned()
    {
        // Arrange
        await SetupDatabase(
            32,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        // Act
        var result = await Sut.ExecuteAsync(
            new CompareTvShowPlexLibraryCommand(remoteLibrary.Id, ownedLibrary.Id),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldUpdateExistingComparisonScopeSnapshots_WhenScopeAlreadyExists()
    {
        // Arrange
        await SetupDatabase(34, config =>
        {
            config.PlexServerCount = 2;
            config.PlexTvShowLibraryCount = 1;
            config.PlexAccountCount = 1;
        });

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        var remoteUpdatedAt = new DateTime(2026, 7, 21, 17, 24, 15, DateTimeKind.Utc);
        var ownedUpdatedAt = new DateTime(2026, 7, 21, 14, 8, 33, DateTimeKind.Utc);
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, remoteUpdatedAt);
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, ownedUpdatedAt);
        dbContext.PlexComparisonScopes.Add(new PlexComparisonState
        {
            Id = 0,
            RemotePlexLibraryId = remoteLibrary.Id,
            OwnedPlexLibraryId = ownedLibrary.Id,
            MediaType = PlexMediaType.TvShow,
            CompletedAt = new DateTime(2026, 7, 20, 22, 19, 16, DateTimeKind.Utc),
            RemoteLibraryUpdatedAt = new DateTime(2026, 7, 20, 14, 7, 34, DateTimeKind.Utc),
            OwnedLibraryUpdatedAt = new DateTime(2026, 7, 20, 20, 13, 32, DateTimeKind.Utc),
        });
        await dbContext.SaveChangesNewAsync(CancellationToken);

        // Act
        var result = await Sut.ExecuteAsync(
            new CompareTvShowPlexLibraryCommand(remoteLibrary.Id, ownedLibrary.Id),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var scope = await IDbContext.PlexComparisonScopes.SingleAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id
                 && x.OwnedPlexLibraryId == ownedLibrary.Id
                 && x.MediaType == PlexMediaType.TvShow,
            CancellationToken
        );
        scope.RemoteLibraryUpdatedAt.ShouldBe(remoteUpdatedAt);
        scope.OwnedLibraryUpdatedAt.ShouldBe(ownedUpdatedAt);
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
}
