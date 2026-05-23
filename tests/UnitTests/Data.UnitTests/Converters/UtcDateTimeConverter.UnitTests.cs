namespace Reaparr.Data.UnitTests;

public class UtcDateTimeConverterUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldPersistPlexLibraryContentChangedAtRawValue_WhenSavedAndLoaded()
    {
        // Arrange
        await SetupDatabase(27300);
        const long contentChangedAt = 123456789;

        await SaveLibrary(contentChangedAt);

        // Act
        var loadedContentChangedAt = await LoadContentChangedAt();

        // Assert
        loadedContentChangedAt.ShouldBe(contentChangedAt);
    }

    [Test]
    public async Task ShouldPersistZeroPlexLibraryContentChangedAt_WhenUsingDefaultValue()
    {
        // Arrange
        await SetupDatabase(27301);

        await SaveLibrary();

        // Act
        var loadedContentChangedAt = await LoadContentChangedAt();

        // Assert
        loadedContentChangedAt.ShouldBe(0);
    }

    private async Task SaveLibrary(long? contentChangedAt = null)
    {
        await using var dbContext = (ReaparrDbContext)IDbContext;
        var server = FakeData.GetPlexServer(new Seed(27304)).Generate();
        dbContext.PlexServers.Add(server);
        await dbContext.SaveChangesAsync(CancellationToken);

        var library = new PlexLibrary
        {
            Type = PlexMediaType.Movie,
            Title = "UTC converter test library",
            Key = "utc-converter-test-library",
            CreatedAt = null,
            UpdatedAt = null,
            ScannedAt = null,
            Uuid = Guid.NewGuid().ToString(),
            Language = "en",
            PlexServerId = server.Id,
        };

        if (contentChangedAt.HasValue)
            library.ContentChangedAt = contentChangedAt.Value;

        dbContext.PlexLibraries.Add(library);
        await dbContext.SaveChangesAsync(CancellationToken);
    }

    private async Task<long> LoadContentChangedAt()
    {
        await using var dbContext = (ReaparrDbContext)IDbContext;

        return await dbContext.PlexLibraries.Select(x => x.ContentChangedAt).SingleAsync(CancellationToken);
    }
}
