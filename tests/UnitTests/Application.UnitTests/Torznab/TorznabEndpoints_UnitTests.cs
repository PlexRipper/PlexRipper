using Application.Contracts;
using PlexRipper.Application.Contracts;
using Settings.Contracts;
using Shouldly;
using Xunit;

namespace PlexRipper.Application.UnitTests;

public class TorznabEndpoints_UnitTests : BaseUnitTest<TorznabSearchEndpoint>
{
    public TorznabEndpoints_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task TorznabSearchEndpoint_WhenDisabled_ShouldReturn503()
    {
        // Arrange
        var userSettings = GetMock<IUserSettings>();
        userSettings.Setup(x => x.TorznabSettings.IsEnabled).Returns(false);

        var endpoint = new TorznabSearchEndpoint(userSettings.Object, DbContext);
        var request = new TorznabSearchRequest
        {
            T = "search",
            Q = "test movie",
            ApiKey = "test-api-key"
        };

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
        {
            await endpoint.ExecuteAsync(request, CancellationToken.None);
        });
    }

    [Fact]
    public async Task TorznabSearchEndpoint_WhenInvalidApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var userSettings = GetMock<IUserSettings>();
        userSettings.Setup(x => x.TorznabSettings.IsEnabled).Returns(true);
        userSettings.Setup(x => x.TorznabSettings.ApiKey).Returns("correct-api-key");

        var endpoint = new TorznabSearchEndpoint(userSettings.Object, DbContext);
        var request = new TorznabSearchRequest
        {
            T = "search",
            Q = "test movie",
            ApiKey = "wrong-api-key"
        };

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
        {
            await endpoint.ExecuteAsync(request, CancellationToken.None);
        });
    }

    [Fact]
    public void TorznabXmlFormatter_ShouldSerializeCapabilities()
    {
        // Arrange
        var capabilities = TorznabXmlFormatter.CreateDefaultCapabilities("http://localhost:8080", 100);

        // Act
        var xml = TorznabXmlFormatter.SerializeToXml(capabilities);

        // Assert
        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("<caps>");
        xml.ShouldContain("<server");
        xml.ShouldContain("PlexRipper");
        xml.ShouldContain("<limits");
        xml.ShouldContain("max=\"100\"");
    }

    [Fact]
    public void PlexToTorznabMapper_ShouldMapMovieCorrectly()
    {
        // Arrange
        var movie = DataGenerate.PlexMovie;
        movie.Title = "Test Movie";
        movie.Year = 2023;
        movie.MediaSize = 5_000_000_000; // 5GB

        // Act
        var torznabItem = movie.ToTorznabItem("http://download-url", "http://base-url");

        // Assert
        torznabItem.ShouldNotBeNull();
        torznabItem.Title.ShouldContain("Test Movie");
        torznabItem.Title.ShouldContain("(2023)");
        torznabItem.Size.ShouldBe(5_000_000_000);
        
        var categoryAttr = torznabItem.Attributes.FirstOrDefault(a => a.Name == "category");
        categoryAttr.ShouldNotBeNull();
        categoryAttr.Value.ShouldBe("2000"); // Movies category
    }
}