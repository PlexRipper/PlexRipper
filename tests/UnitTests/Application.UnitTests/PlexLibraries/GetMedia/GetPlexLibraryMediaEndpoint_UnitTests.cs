using Application.Contracts;
using Application.Contracts.Validators;

namespace PlexRipper.Application.UnitTests;

public class GetPlexLibraryMediaEndpoint_UnitTests : BaseUnitTest<GetPlexLibraryMediaEndpoint>
{
    private PlexMediaSlimDTOValidator PlexMediaSlimDtoValidator => new();

    public GetPlexLibraryMediaEndpoint_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveAllThePlexLibraryMedia_WhenPageAndSizeAreNotSetAndMediaIsMovies()
    {
        // Arrange
        var movieCount = 100;
        await SetupDatabase(
            39919,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexServerConnectionPerServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.MovieCount = movieCount;
            }
        );

        var request = new GetPlexLibraryMediaEndpointRequest
        {
            PlexLibraryId = 1,
            Page = 0,
            Size = 0,
        };

        // Act
        var ep = SetupEndpointUnitTest<GetPlexLibraryMediaEndpoint>();
        await ep.HandleAsync(request, CancellationToken.None);
        var result = ep.Response as ResultDTO<PlexMediaStatisticsDTO>;

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();
        result.Value.MovieCount.ShouldBe(movieCount);
        result.Value.TvShowCount.ShouldBe(0);
        result.Value.SeasonCount.ShouldBe(0);
        result.Value.EpisodeCount.ShouldBe(0);
        result.Value.MediaCount.ShouldBe(result.Value.MediaList.Count);
        foreach (var mediaSlimDTO in result.Value.MediaList)
        {
            var validationResult = await PlexMediaSlimDtoValidator.ValidateAsync(
                mediaSlimDTO,
                TestContext.Current.CancellationToken
            );
            validationResult.Errors.ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task ShouldHaveAllThePlexLibraryMedia_WhenPageAndSizeAreNotSetAndMediaIsTvShows()
    {
        // Arrange
        await SetupDatabase(
            69592,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 100;
                config.TvShowSeasonCount = 3;
                config.TvShowEpisodeCount = 5;
            }
        );

        var request = new GetPlexLibraryMediaEndpointRequest
        {
            PlexLibraryId = 1,
            Page = 0,
            Size = 0,
        };

        // Act
        var ep = SetupEndpointUnitTest<GetPlexLibraryMediaEndpoint>();
        await ep.HandleAsync(request, CancellationToken.None);
        var result = ep.Response as ResultDTO<PlexMediaStatisticsDTO>;

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();
        result.Value.MovieCount.ShouldBe(0);
        result.Value.TvShowCount.ShouldBe(100);
        result.Value.SeasonCount.ShouldBe(100 * 3);
        result.Value.EpisodeCount.ShouldBe(100 * 3 * 5);
        result.Value.MediaCount.ShouldBe(result.Value.MediaList.Count);

        foreach (var plexMediaSlimDto in result.Value.MediaList)
        {
            var validationResult = await PlexMediaSlimDtoValidator.ValidateAsync(
                plexMediaSlimDto,
                TestContext.Current.CancellationToken
            );
            validationResult.Errors.ShouldBeEmpty();
        }
    }
}
