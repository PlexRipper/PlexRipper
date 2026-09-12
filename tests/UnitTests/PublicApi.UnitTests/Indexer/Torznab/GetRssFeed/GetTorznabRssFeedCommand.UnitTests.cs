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
            Attributes = [],
            IncludeAllAttributes = true,
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
            Attributes = [],
            IncludeAllAttributes = true,
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
            Attributes = [],
            IncludeAllAttributes = true,
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
            Attributes = [],
            IncludeAllAttributes = true,
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

    [Test]
    public void ShouldValidate_WhenLimitExceedsPreviousMaximum()
    {
        // Arrange
        var validator = new GetTorznabRssFeedCommandValidator();
        var command = new GetTorznabRssFeedCommand
        {
            Integration = new IntegrationIdentity(IntegrationType.Radarr, Guid.NewGuid()),
            Categories = [],
            IncludeMovies = true,
            IncludeEpisodes = false,
            Limit = 1000,
            Offset = 0,
            TorznabApiKey = "rss-key",
            Attributes = [],
            IncludeAllAttributes = true,
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldUseKnownCategory_WhenKnownAndUnknownCategoriesAreRequested()
    {
        // Arrange
        await SetupDatabase(
            7635,
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
        var category = (await IDbContext.PlexMovieData.FirstAsync(CancellationToken)).ToTorznabMovieCategory();
        var command = new GetTorznabRssFeedCommand
        {
            Integration = integration,
            Categories = [category, 9999],
            IncludeMovies = true,
            IncludeEpisodes = false,
            Limit = 50,
            Offset = 0,
            TorznabApiKey = "rss-key",
            Attributes = [],
            IncludeAllAttributes = true,
        };

        // Act
        var result = await TestHandlerExecuteAsync<TorznabMediaSearchResponseDTO>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Channel.Items.ShouldNotBeEmpty();
        result
            .Value.Channel.Items.All(x => x.Attributes.Any(a => a.Name == "category" && a.Value == category.ToString()))
            .ShouldBeTrue();
    }

    [Test]
    public async Task ShouldNotDuplicateItems_WhenParentAndChildCategoriesAreRequested()
    {
        // Arrange
        await SetupDatabase(
            7636,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 3;
                config.RadarrIntegrationCount = 1;
            }
        );
        var integration = (await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken)).Id.ToRadarrIdentity();
        var totalParts = await IDbContext.PlexMovieData.CountAsync(CancellationToken);
        var command = new GetTorznabRssFeedCommand
        {
            Integration = integration,
            Categories = [(int)TorznabCategoryId.Movies, (int)TorznabCategoryId.Movies_HD],
            IncludeMovies = true,
            IncludeEpisodes = false,
            Limit = 50,
            Offset = 0,
            TorznabApiKey = "rss-key",
            Attributes = [],
            IncludeAllAttributes = true,
        };

        // Act
        var result = await TestHandlerExecuteAsync<TorznabMediaSearchResponseDTO>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Channel.Response.Total.ShouldBe(totalParts);
        result.Value.Channel.Items.Select(x => x.Guid.Value).Distinct().Count().ShouldBe(totalParts);
    }

    [Test]
    public async Task ShouldReturnEmpty_WhenMovieSearchUsesOnlyTvCategories()
    {
        // Arrange
        await SetupDatabase(
            7637,
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
            Categories = [(int)TorznabCategoryId.TV],
            IncludeMovies = true,
            IncludeEpisodes = false,
            Limit = 50,
            Offset = 0,
            TorznabApiKey = "rss-key",
            Attributes = [],
            IncludeAllAttributes = true,
        };

        // Act
        var result = await TestHandlerExecuteAsync<TorznabMediaSearchResponseDTO>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Channel.Response.Total.ShouldBe(0);
        result.Value.Channel.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldReturnFilteredTotalBeforePaging()
    {
        // Arrange
        await SetupDatabase(
            7638,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 4;
                config.RadarrIntegrationCount = 1;
            }
        );
        var integration = (await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken)).Id.ToRadarrIdentity();
        var category = (await IDbContext.PlexMovieData.FirstAsync(CancellationToken)).ToTorznabMovieCategory();
        var expectedTotal = (await IDbContext.PlexMovieData.ToListAsync(CancellationToken)).Count(x =>
            x.ToTorznabMovieCategory() == category
        );
        var command = new GetTorznabRssFeedCommand
        {
            Integration = integration,
            Categories = [category],
            IncludeMovies = true,
            IncludeEpisodes = false,
            Limit = 1,
            Offset = 1,
            TorznabApiKey = "rss-key",
            Attributes = [],
            IncludeAllAttributes = true,
        };

        // Act
        var result = await TestHandlerExecuteAsync<TorznabMediaSearchResponseDTO>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Channel.Response.Total.ShouldBe(expectedTotal);
        result.Value.Channel.Items.Count.ShouldBe(Math.Min(1, Math.Max(0, expectedTotal - 1)));
    }

    [Test]
    public async Task ShouldReturnEmpty_WhenTvSearchUsesOnlyMovieCategories()
    {
        // Arrange
        await SetupDatabase(
            7639,
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
            Categories = [(int)TorznabCategoryId.Movies],
            IncludeMovies = false,
            IncludeEpisodes = true,
            Limit = 50,
            Offset = 0,
            TorznabApiKey = "rss-key",
            Attributes = [],
            IncludeAllAttributes = true,
        };

        // Act
        var result = await TestHandlerExecuteAsync<TorznabMediaSearchResponseDTO>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Channel.Response.Total.ShouldBe(0);
        result.Value.Channel.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldReturnOnlyRequestedAttributes_WhenExtendedIsDisabled()
    {
        // Arrange
        await SetupDatabase(7640, config =>
        {
            config.PlexServerCount = 1;
            config.PlexAccountCount = 1;
            config.PlexMovieLibraryCount = 1;
            config.MovieCount = 1;
            config.RadarrIntegrationCount = 1;
        });
        var integration = (await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken)).Id.ToRadarrIdentity();
        var command = new GetTorznabRssFeedCommand
        {
            Integration = integration,
            Categories = [],
            IncludeMovies = true,
            IncludeEpisodes = false,
            Limit = 50,
            Offset = 0,
            TorznabApiKey = "rss-key",
            Attributes = ["category", "SIZE", "unknown"],
            IncludeAllAttributes = false,
        };

        // Act
        var result = await TestHandlerExecuteAsync<TorznabMediaSearchResponseDTO>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Channel.Items.ShouldNotBeEmpty();
        result.Value.Channel.Items.All(x => x.Attributes.Select(a => a.Name).Order().SequenceEqual(new[] { "category", "size" })).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnAllAttributes_WhenExtendedIsEnabled()
    {
        // Arrange
        await SetupDatabase(7641, config =>
        {
            config.PlexServerCount = 1;
            config.PlexAccountCount = 1;
            config.PlexMovieLibraryCount = 1;
            config.MovieCount = 1;
            config.RadarrIntegrationCount = 1;
        });
        var integration = (await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken)).Id.ToRadarrIdentity();
        var command = new GetTorznabRssFeedCommand
        {
            Integration = integration,
            Categories = [],
            IncludeMovies = true,
            IncludeEpisodes = false,
            Limit = 50,
            Offset = 0,
            TorznabApiKey = "rss-key",
            Attributes = ["category"],
            IncludeAllAttributes = true,
        };

        // Act
        var result = await TestHandlerExecuteAsync<TorznabMediaSearchResponseDTO>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var names = result.Value.Channel.Items.Single().Attributes.Select(x => x.Name).ToList();
        names.ShouldContain("category");
        names.ShouldContain("size");
        names.ShouldContain("seeders");
        names.ShouldContain("resolution");
        names.ShouldContain("uploadvolumefactor");
        names.ShouldContain("tag");
        result.Value.Channel.Items.Single().Attributes.Single(x => x.Name == "tag").Value.ShouldNotBeEmpty();
    }
}
