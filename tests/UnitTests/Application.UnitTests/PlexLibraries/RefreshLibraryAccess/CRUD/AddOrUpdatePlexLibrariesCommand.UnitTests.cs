using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class AddOrUpdatePlexLibrariesCommandUnitTests : BaseUnitTest<AddOrUpdatePlexLibrariesCommandHandler>
{
    public AddOrUpdatePlexLibrariesCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldAddAllPlexLibraries_WhenNoneExistInTheDatabase()
    {
        // Arrange
        var serverCount = 5;
        var libraryCount = 5;
        var seed = await SetupDatabase(
            32,
            config =>
            {
                config.PlexServerCount = serverCount;
                config.PlexAccountCount = 1;
            }
        );

        var plexAccount = IDbContext.PlexAccounts.FirstOrDefault();
        plexAccount.ShouldNotBeNull();
        var plexServers = await IDbContext.PlexServers.ToListAsync(CancellationToken);
        plexServers.ShouldNotBeNull();

        var plexLibraries = new List<PlexLibrary>();
        foreach (var plexServer in plexServers)
        {
            var list = FakeData.GetPlexLibrary(seed).Generate(libraryCount);
            foreach (var plexLibrary in list)
                plexLibrary.PlexServerId = plexServer.Id;

            plexLibraries.AddRange(list);
        }

        // Act
        var request = new AddOrUpdatePlexLibrariesCommand
        {
            PlexAccountId = plexAccount.Id,
            PlexLibraries = plexLibraries,
        };
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(serverCount);
        foreach (var rapport in result.Value)
        {
            rapport.GetGranted.Count.ShouldBe(libraryCount);
            rapport.GetUpdated.Count.ShouldBe(0);
            rapport.GetRevoked.Count.ShouldBe(0);
        }

        var plexLibrariesDb = IDbContext.PlexLibraries.ToList();
        plexLibrariesDb.Count.ShouldBe(serverCount * libraryCount);
        var plexAccountLibrariesDb = IDbContext.PlexAccountLibraries.ToList();
        plexAccountLibrariesDb.Count.ShouldBe(serverCount * libraryCount);

        foreach (var expectedPlexLibrary in plexLibraries)
        {
            var plexLibraryDb = plexLibrariesDb.Find(x => x.Key == expectedPlexLibrary.Key);
            plexLibraryDb.ShouldNotBeNull();
        }

        foreach (var plexAccountLibrary in plexAccountLibrariesDb)
        {
            plexAccountLibrary.PlexAccountId.ShouldBe(plexAccount.Id);
            plexAccountLibrary.PlexServerId.ShouldBeInRange(1, serverCount);
            plexAccountLibrary.PlexLibraryId.ShouldBeInRange(1, serverCount * libraryCount);
        }
    }

    [Fact]
    public async Task ShouldUpdatePlexLibraries_WhenTheyExistInTheDatabase()
    {
        // Arrange
        var serverCount = 5;
        var libraryCount = 5;
        await SetupDatabase(
            32,
            config =>
            {
                config.PlexServerCount = serverCount;
                config.PlexMovieLibraryCount = libraryCount;
                config.PlexAccountCount = 1;
            }
        );

        var dbContext = IDbContext;
        var plexAccount = dbContext.PlexAccounts.FirstOrDefault();
        plexAccount.ShouldNotBeNull();
        var plexServers = await dbContext.PlexServers.ToListAsync(CancellationToken);
        plexServers.ShouldNotBeNull();

        // Set values that should not be overwritten by refreshing the libraries
        var syncedAtDateTime = DateTime.Now - TimeSpan.FromHours(6);
        var plexLibraries = dbContext.PlexLibraries.AsTracking().ToList();
        foreach (var plexLibrary in plexLibraries)
        {
            plexLibrary.SyncedAt = syncedAtDateTime;
            plexLibrary.DefaultDestinationId = 5;

            if (plexLibrary.Type == PlexMediaType.Movie)
                plexLibrary.SetMovieMetaData(100, 1000);

            if (plexLibrary.Type == PlexMediaType.TvShow)
                plexLibrary.SetTvShowMetaData(100, 100, 100, 1000);
        }

        await dbContext.SaveChangesAsync(CancellationToken);

        // Create API Data
        var updatedTime = DateTime.Now - TimeSpan.FromHours(4);

        // Act
        var request = new AddOrUpdatePlexLibrariesCommand
        {
            PlexAccountId = plexAccount.Id,
            PlexLibraries = plexLibraries.ToApiLibraries(updatedTime),
        };
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(serverCount);
        foreach (var rapport in result.Value)
        {
            rapport.GetGranted.Count.ShouldBe(0);
            rapport.GetUpdated.Count.ShouldBe(libraryCount);
            rapport.GetRevoked.Count.ShouldBe(0);
        }

        var plexLibrariesDb = IDbContext.PlexLibraries.ToList();
        plexLibrariesDb.Count.ShouldBe(serverCount * libraryCount);
        var plexAccountLibrariesDb = IDbContext.PlexAccountLibraries.ToList();
        plexAccountLibrariesDb.Count.ShouldBe(serverCount * libraryCount);

        foreach (var expectedPlexLibrary in plexLibraries)
        {
            var plexLibraryDb = plexLibrariesDb.Find(x => x.Uuid == expectedPlexLibrary.Uuid);
            plexLibraryDb.ShouldNotBeNull();
            plexLibraryDb.UpdatedAt.ShouldBe(updatedTime);
            plexLibraryDb.SyncedAt.ShouldBe(syncedAtDateTime);
            plexLibraryDb.DefaultDestinationId.ShouldBe(5);

            if (plexLibraryDb.Type == PlexMediaType.Movie)
                plexLibraryDb.MovieCount.ShouldBe(100);

            if (plexLibraryDb.Type == PlexMediaType.TvShow)
            {
                plexLibraryDb.TvShowCount.ShouldBe(100);
                plexLibraryDb.SeasonCount.ShouldBe(100);
                plexLibraryDb.EpisodeCount.ShouldBe(100);
            }

            plexLibraryDb.MediaSize.ShouldBe(1000);
        }

        foreach (var plexAccountLibrary in plexAccountLibrariesDb)
        {
            plexAccountLibrary.PlexAccountId.ShouldBe(plexAccount.Id);
            plexAccountLibrary.PlexServerId.ShouldBeInRange(1, serverCount);
            plexAccountLibrary.PlexLibraryId.ShouldBeInRange(1, serverCount * libraryCount);
        }
    }

    [Fact]
    public async Task ShouldDeletePlexLibraryAccess_WhenThePlexServerHasNoPlexLibraries()
    {
        // Arrange
        var serverCount = 5;
        var libraryCount = 6;
        await SetupDatabase(
            32,
            config =>
            {
                config.PlexServerCount = serverCount;
                config.PlexMovieLibraryCount = libraryCount;
                config.PlexAccountCount = 1;
            }
        );

        var dbContext = IDbContext;
        var plexAccount = dbContext.PlexAccounts.FirstOrDefault();
        plexAccount.ShouldNotBeNull();
        var plexLibraries = dbContext.PlexLibraries.AsTracking().ToList();

        // Remove even numbered plexLibraries
        for (var i = plexLibraries.Count - 1; i >= 0; i--)
            if (i % 2 == 0)
                plexLibraries.RemoveAt(i);

        var updatedTime = DateTime.Now - TimeSpan.FromHours(2);
        var request = new AddOrUpdatePlexLibrariesCommand
        {
            PlexAccountId = plexAccount.Id,
            PlexLibraries = plexLibraries.ToApiLibraries(updatedTime),
        };

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(serverCount);
        foreach (var rapport in result.Value)
        {
            rapport.GetGranted.Count.ShouldBe(0);
            rapport.GetUpdated.ShouldAllBe(x => x.PlexLibraryId % 2 == 0);
            rapport.GetRevoked.ShouldAllBe(x => x.PlexLibraryId % 2 != 0);
        }

        var plexLibrariesDb = IDbContext.PlexLibraries.ToList();
        plexLibrariesDb.Count.ShouldBe(serverCount * libraryCount);
        var plexAccountLibrariesDb = IDbContext.PlexAccountLibraries.ToList();
        plexAccountLibrariesDb.Count.ShouldBe(request.PlexLibraries.Count);

        plexAccountLibrariesDb.Select(x => x.PlexAccountId).ShouldAllBe(x => x == plexAccount.Id);
        plexAccountLibrariesDb.Select(x => x.PlexServerId).ShouldAllBe(x => x > 0 && x <= serverCount);
        plexAccountLibrariesDb.Select(x => x.PlexLibraryId).ShouldAllBe(x => x % 2 == 0);
    }
}
