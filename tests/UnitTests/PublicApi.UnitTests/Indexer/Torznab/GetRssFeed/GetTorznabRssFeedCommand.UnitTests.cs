using Reaparr.Application.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.PublicAPI.UnitTests;

public class GetTorznabRssFeedCommandUnitTests : BaseCommandUnitTest<GetTorznabRssFeedCommand>
{
    public GetTorznabRssFeedCommandUnitTests()
    {
        Mock.Mock<INetworkSettings>().SetupGet(x => x.Url).Returns("http://localhost");
    }

    [Test]
    public async Task ShouldPageMoviePartsByAddedAt_WhenCatalogIsUnchanged()
    {
        // Arrange
        await SetupDatabase(
            7631,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 4;
                config.IncludeMultiPartMovies = true;
                config.RadarrIntegrationCount = 1;
            }
        );
        var integration = (await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken)).Id.ToRadarrIdentity();
        var expected = await IDbContext
            .PlexMovieData.OrderByDescending(x => x.PlexMovie!.AddedAt)
            .ThenByDescending(x => x.PlexServerId)
            .ThenByDescending(x => x.PlexApiMediaId)
            .ThenByDescending(x => x.PlexApiPartId)
            .Select(x => !string.IsNullOrEmpty(x.GeneratedFilename) ? x.GeneratedFilename : x.OriginalFilename)
            .ToListAsync(CancellationToken);
        var command = new GetTorznabRssFeedCommand
        {
            Integration = integration,
            Categories = [],
            IncludeMovies = true,
            IncludeEpisodes = false,
            Limit = 3,
            Offset = 1,
            TorznabApiKey = "rss-key",
        };

        // Act
        var result = await TestHandlerExecuteAsync<TorznabMediaSearchResponseDTO>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Channel.Response.Offset.ShouldBe(1);
        result.Value.Channel.Response.Total.ShouldBe(expected.Count);
        result.Value.Channel.Items.Select(x => x.Title).ShouldBe(expected.Skip(1).Take(3));
        result.Value.Channel.Items.Select(x => x.Guid.Value).Distinct().Count().ShouldBe(3);
        result.Value.Channel.Items.All(x => x.Guid.Value != x.Link).ShouldBeTrue();
        result.Value.Channel.Items.All(x => x.Attributes.Any(a => a.Name == "size")).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnSameGuidAndPublicationDate_WhenFeedIsReadAgain()
    {
        // Arrange
        await SetupDatabase(
            7632,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 2;
                config.SonarrIntegrationCount = 1;
            }
        );
        var integration = (await IDbContext.SonarrIntegrations.SingleAsync(CancellationToken)).Id.ToSonarrIdentity();
        var command = new GetTorznabRssFeedCommand
        {
            Integration = integration,
            Categories = [5000],
            IncludeMovies = false,
            IncludeEpisodes = true,
            Limit = 50,
            Offset = 0,
            TorznabApiKey = "rss-key",
        };

        // Act
        var first = await TestHandlerExecuteAsync<TorznabMediaSearchResponseDTO>(command);
        var second = await TestHandlerExecuteAsync<TorznabMediaSearchResponseDTO>(command);

        // Assert
        first.IsSuccess.ShouldBeTrue();
        second.IsSuccess.ShouldBeTrue();
        first
            .Value.Channel.Items.Select(x => (x.Guid.Value, x.PubDate))
            .ShouldBe(second.Value.Channel.Items.Select(x => (x.Guid.Value, x.PubDate)));
        first.Value.Channel.Items.All(x => x.Attributes.Any(a => a.Name == "category")).ShouldBeTrue();
        first.Value.Channel.Items.All(x => x.Attributes.Any(a => a.Name == "season")).ShouldBeTrue();
        first.Value.Channel.Items.All(x => x.Attributes.Any(a => a.Name == "episode")).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnEmptyFeed_WhenOnlyUnknownCategoriesAreRequested()
    {
        // Arrange
        await SetupDatabase(
            7633,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
                config.RadarrIntegrationCount = 1;
            }
        );
        var integration = (await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken)).Id.ToRadarrIdentity();
        var command = new GetTorznabRssFeedCommand
        {
            Integration = integration,
            Categories = [9999],
            IncludeMovies = true,
            IncludeEpisodes = false,
            Limit = 50,
            Offset = 0,
            TorznabApiKey = "rss-key",
        };

        // Act
        var result = await TestHandlerExecuteAsync<TorznabMediaSearchResponseDTO>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Channel.Response.Total.ShouldBe(0);
        result.Value.Channel.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldGloballyPageMoviesAndEpisodes_WhenBothTypesAreRequested()
    {
        // Arrange
        await SetupDatabase(
            7634,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 2;
                config.RadarrIntegrationCount = 1;
            }
        );
        var integration = (await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken)).Id.ToRadarrIdentity();
        var command = new GetTorznabRssFeedCommand
        {
            Integration = integration,
            Categories = [],
            IncludeMovies = true,
            IncludeEpisodes = true,
            Limit = 3,
            Offset = 1,
            TorznabApiKey = "rss-key",
        };

        // Act
        var result = await TestHandlerExecuteAsync<TorznabMediaSearchResponseDTO>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Channel.Response.Total.ShouldBe(4);
        result.Value.Channel.Items.Count.ShouldBe(3);
        result.Value.Channel.Items.Select(x => x.Attributes.Single(a => a.Name == "type").Value).ShouldContain("movie");
        result
            .Value.Channel.Items.Select(x => x.Attributes.Single(a => a.Name == "type").Value)
            .ShouldContain("series");
    }
}
