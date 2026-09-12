namespace Reaparr.PublicAPI.UnitTests;

public class TorznabRequestUnitTests
{
    [Test]
    public void ShouldMapEndpointValuesAndDefaults_WhenCreatingTypedRequest()
    {
        // Arrange
        var endpointRequest = new TorznabEndpointRequest
        {
            Type = "tvsearch",
            Query = "Silo",
            Season = 2,
            Episode = 3,
            TvdbId = 123,
            ApiKey = "key",
            Categories = [5030],
        };

        // Act
        var request = endpointRequest.ToTorznabRequest();

        // Assert
        request.Type.ShouldBe(TorznabQueryType.TvSearch);
        request.Query.ShouldBe("Silo");
        request.Season.ShouldBe(2);
        request.Episode.ShouldBe(3);
        request.TvdbId.ShouldBe(123);
        request.Limit.ShouldBe(100);
        request.Offset.ShouldBe(0);
        request.Categories.ShouldBe([5030]);
        request.Mode.ShouldBe(TorznabRequestMode.ActiveSearch);
    }

    [Test]
    [Arguments("caps", TorznabQueryType.Caps)]
    [Arguments("search", TorznabQueryType.Search)]
    [Arguments("tvsearch", TorznabQueryType.TvSearch)]
    [Arguments("movie", TorznabQueryType.Movie)]
    [Arguments("TVSEARCH", TorznabQueryType.TvSearch)]
    public void ShouldParseSupportedTypeWithoutCaseSensitivity(string value, TorznabQueryType expected)
    {
        // Arrange
        var endpointRequest = new TorznabEndpointRequest { Type = value, ApiKey = "key" };

        // Act
        var request = endpointRequest.ToTorznabRequest();

        // Assert
        request.Type.ShouldBe(expected);
        endpointRequest.ParsedType.ShouldBe(expected);
    }

    [Test]
    public void ShouldRejectUnsupportedType()
    {
        // Arrange
        var endpointRequest = new TorznabEndpointRequest { Type = "book", ApiKey = "key" };

        // Assert
        endpointRequest.ParsedType.ShouldBe(TorznabQueryType.Unknown);
    }

    [Test]
    public void ShouldDeriveRssMediaTypesFromQueryTypeAndCategories()
    {
        // Arrange
        var movieEndpointRequest = new TorznabEndpointRequest { Type = "movie", ApiKey = "key" };
        var tvEndpointRequest = new TorznabEndpointRequest { Type = "tvsearch", ApiKey = "key" };
        var mixedEndpointRequest = new TorznabEndpointRequest { Type = "search", ApiKey = "key" };

        // Act
        var movie = movieEndpointRequest.ToTorznabRequest();
        var tv = tvEndpointRequest.ToTorznabRequest();
        var mixed = mixedEndpointRequest.ToTorznabRequest();

        // Assert
        movie.Mode.ShouldBe(TorznabRequestMode.Rss);
        movie.IncludesMovies.ShouldBeTrue();
        movie.IncludesEpisodes.ShouldBeFalse();
        tv.IncludesMovies.ShouldBeFalse();
        tv.IncludesEpisodes.ShouldBeTrue();
        mixed.IncludesMovies.ShouldBeTrue();
        mixed.IncludesEpisodes.ShouldBeTrue();
    }
}
