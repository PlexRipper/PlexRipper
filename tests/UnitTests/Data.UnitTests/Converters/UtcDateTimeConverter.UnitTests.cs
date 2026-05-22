namespace Reaparr.Data.UnitTests;

public class UtcDateTimeConverterUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldLoadDateTimeAsUtc_WhenSavedAsUtc()
    {
        // Arrange
        await SetupDatabase(27300);
        var savedDateTime = new DateTime(2026, 5, 22, 10, 30, 0, DateTimeKind.Utc);

        await SaveLibrary(savedDateTime);

        // Act
        var loadedDateTime = await LoadContentChangedAt();

        // Assert
        loadedDateTime.Kind.ShouldBe(DateTimeKind.Utc);
        loadedDateTime.ShouldBe(savedDateTime);
    }

    [Test]
    public async Task ShouldConvertDateTimeToUtc_WhenSavedAsLocal()
    {
        // Arrange
        await SetupDatabase(27301);
        var savedDateTime = new DateTime(2026, 5, 22, 10, 30, 0, DateTimeKind.Local);
        var expectedDateTime = savedDateTime.ToUniversalTime();

        await SaveLibrary(savedDateTime);

        // Act
        var loadedDateTime = await LoadContentChangedAt();

        // Assert
        loadedDateTime.Kind.ShouldBe(DateTimeKind.Utc);
        loadedDateTime.ShouldBe(expectedDateTime);
    }

    [Test]
    public async Task ShouldTreatDateTimeAsUtc_WhenSavedAsUnspecified()
    {
        // Arrange
        await SetupDatabase(27302);
        var savedDateTime = new DateTime(2026, 5, 22, 10, 30, 0, DateTimeKind.Unspecified);
        var expectedDateTime = DateTime.SpecifyKind(savedDateTime, DateTimeKind.Utc);

        await SaveLibrary(savedDateTime);

        // Act
        var loadedDateTime = await LoadContentChangedAt();

        // Assert
        loadedDateTime.Kind.ShouldBe(DateTimeKind.Utc);
        loadedDateTime.ShouldBe(expectedDateTime);
    }

    [Test]
    public async Task ShouldLoadDateTimeMinValueAsUtc_WhenUsingDefaultValue()
    {
        // Arrange
        await SetupDatabase(27303);

        await SaveLibrary();

        // Act
        var loadedDateTime = await LoadContentChangedAt();

        // Assert
        loadedDateTime.Kind.ShouldBe(DateTimeKind.Utc);
        loadedDateTime.ShouldBe(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc));
    }

    private async Task SaveLibrary(DateTime? contentChangedAt = null)
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

    private async Task<DateTime> LoadContentChangedAt()
    {
        await using var dbContext = (ReaparrDbContext)IDbContext;

        return await dbContext.PlexLibraries.Select(x => x.ContentChangedAt).SingleAsync(CancellationToken);
    }
}
