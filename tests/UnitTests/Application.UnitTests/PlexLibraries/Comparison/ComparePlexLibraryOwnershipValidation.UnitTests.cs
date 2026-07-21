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

    private async Task SetOwnedOverrideAsync(int plexServerId, bool ownedOverride)
    {
        await IDbContext.PlexServers
            .Where(x => x.Id == plexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, ownedOverride), CancellationToken);
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

    private async Task SetOwnedOverrideAsync(int plexServerId, bool ownedOverride)
    {
        await IDbContext.PlexServers
            .Where(x => x.Id == plexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, ownedOverride), CancellationToken);
    }
}
