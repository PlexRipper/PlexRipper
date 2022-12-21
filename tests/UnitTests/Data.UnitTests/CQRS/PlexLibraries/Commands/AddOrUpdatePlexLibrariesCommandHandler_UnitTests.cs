using PlexRipper.Application;
using PlexRipper.Data.PlexLibraries;

namespace Data.UnitTests.PlexLibraries;

public class AddOrUpdatePlexLibrariesCommandHandler_UnitTests : BaseUnitTest
{
    public AddOrUpdatePlexLibrariesCommandHandler_UnitTests(ITestOutputHelper output) : base(output, true) { }

    [Fact]
    public async Task ShouldAddAllPlexLibraries_WhenNoneExistInTheDatabase()
    {
        // Arrange
        var plexServer = FakeData.GetPlexServer(config => config.Seed = 656324).Generate();
        var plexLibraries = FakeData.GetPlexLibrary(config => config.Seed = 656324).Generate(5);
        var plexAccount = FakeData.GetPlexAccount(config => config.Seed = 353234).Generate();

        DbContext.PlexAccounts.Add(plexAccount);
        DbContext.PlexServers.Add(plexServer);
        SaveChanges();

        foreach (var plexLibrary in plexLibraries)
            plexLibrary.PlexServerId = plexServer.Id;

        // Act
        var request = new AddOrUpdatePlexLibrariesCommand(plexAccount.Id, plexLibraries);
        var handler = new AddOrUpdatePlexLibrariesCommandHandler(DbContext);
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
        var plexServer = FakeData.GetPlexServer(config => config.Seed = 656324).Generate();
        var plexLibraries = FakeData.GetPlexLibrary(config => config.Seed = 656324).Generate(5);
        var plexAccount = FakeData.GetPlexAccount(config => config.Seed = 353234).Generate();
        var updatedTime = DateTime.Now;
        DbContext.PlexAccounts.Add(plexAccount);
        DbContext.PlexServers.Add(plexServer);
        SaveChanges();

        foreach (var plexLibrary in plexLibraries)
            plexLibrary.PlexServerId = plexServer.Id;

        DbContext.PlexLibraries.AddRange(plexLibraries);
        SaveChanges();

        DbContext.PlexAccountLibraries.AddRange(plexLibraries.Select(x => new PlexAccountLibrary()
        {
            PlexAccountId = plexAccount.Id,
            PlexServerId = plexServer.Id,
            PlexLibraryId = x.Id,
        }));
        SaveChanges();
        ResetDbContext();

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
        var handler = new AddOrUpdatePlexLibrariesCommandHandler(GetDbContext());
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