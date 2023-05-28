using Data.Contracts;
using PlexRipper.Data.PlexLibraries;

namespace Data.UnitTests.PlexLibraries;

public class AddOrUpdatePlexLibrariesCommandHandler_UnitTests : BaseUnitTest
{
    public AddOrUpdatePlexLibrariesCommandHandler_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldAddAllPlexLibraries_WhenNoneExistInTheDatabase()
    {
        // Arrange
        Seed = 353234;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexAccountCount = 1;
        });

        var plexAccount = DbContext.PlexAccounts.FirstOrDefault();
        var plexServer = DbContext.PlexServers.FirstOrDefault();

        var plexLibraries = FakeData.GetPlexLibrary(656324).Generate(5);
        foreach (var plexLibrary in plexLibraries)
            plexLibrary.PlexServerId = plexServer.Id;

        // Act
        var request = new AddOrUpdatePlexLibrariesCommand(plexAccount.Id, plexLibraries);
        var handler = new AddOrUpdatePlexLibrariesCommandHandler(_log, DbContext);
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        ResetDbContext();
        result.IsSuccess.ShouldBeTrue();
        var plexLibrariesDb = DbContext.PlexLibraries.ToList();
        plexLibrariesDb.Count.ShouldBe(5);
        var plexAccountLibrariesDb = DbContext.PlexAccountLibraries.ToList();
        plexAccountLibrariesDb.Count.ShouldBe(5);

        foreach (var expectedPlexLibrary in plexLibraries)
        {
            var plexLibraryDb = plexLibrariesDb.Find(x => x.Key == expectedPlexLibrary.Key);
            plexLibraryDb.ShouldNotBeNull();
        }

        foreach (var plexAccountLibrary in plexAccountLibrariesDb)
        {
            plexAccountLibrary.PlexAccountId.ShouldBe(plexAccount.Id);
            plexAccountLibrary.PlexServerId.ShouldBe(plexServer.Id);
        }
    }

    [Fact]
    public async Task ShouldUpdatePlexLibraries_WhenTheyExistInTheDatabase()
    {
        // Arrange
        Seed = 656324;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 5;
            config.PlexAccountCount = 1;
        });

        var plexServer = DbContext.PlexServers.FirstOrDefault();
        var plexLibraries = DbContext.PlexLibraries.ToList();
        var plexAccount = DbContext.PlexAccounts.FirstOrDefault();
        var updatedTime = DateTime.Now;

        // Mimic API data
        foreach (var plexLibrary in plexLibraries)
        {
            plexLibrary.Id = 0;
            plexLibrary.PlexServerId = plexServer.Id;
            plexLibrary.SetNull();
            plexLibrary.UpdatedAt = updatedTime;
        }

        // Act
        var request = new AddOrUpdatePlexLibrariesCommand(plexAccount.Id, plexLibraries);
        var handler = new AddOrUpdatePlexLibrariesCommandHandler(_log, GetDbContext());
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        ResetDbContext();
        result.IsSuccess.ShouldBeTrue();
        var plexLibrariesDb = DbContext.PlexLibraries.ToList();
        plexLibrariesDb.Count.ShouldBe(5);
        var plexAccountLibrariesDb = DbContext.PlexAccountLibraries.ToList();
        plexAccountLibrariesDb.Count.ShouldBe(5);

        foreach (var expectedPlexLibrary in plexLibraries)
        {
            var plexLibraryDb = plexLibrariesDb.Find(x => x.Key == expectedPlexLibrary.Key);
            plexLibraryDb.ShouldNotBeNull();
            plexLibraryDb.UpdatedAt.ShouldBe(updatedTime);
        }

        foreach (var plexAccountLibrary in plexAccountLibrariesDb)
        {
            plexAccountLibrary.PlexAccountId.ShouldBe(plexAccount.Id);
            plexAccountLibrary.PlexServerId.ShouldBe(plexServer.Id);
        }
    }
}