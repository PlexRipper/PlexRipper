using Reaparr.Application.Contracts;
using Reaparr.Application.Contracts.Validators;

namespace Reaparr.Application.UnitTests;

public class GetMediaDetailByIdEndpointUnitTests : BaseUnitTest<GetMediaDetailByIdEndpoint>
{
    private PlexMediaDTOValidator PlexMediaDtoValidator => new();

    public GetMediaDetailByIdEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHavePlexMediaData_WhenValidMediaIdAndPlexMediaTypeMovieIsRequested()
    {
        // Arrange
        var movieCount = 10;
        await SetupDatabase(
            45588,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.MovieCount = movieCount;
            }
        );

        var testMovie = IDbContext.PlexMovies.FirstOrDefault(x => x.HasThumb);
        testMovie.ShouldNotBeNull();

        var request = new GetMediaDetailByIdEndpointRequest(testMovie.Id, PlexMediaType.Movie);

        // Act
        var ep = SetupEndpointUnitTest<GetMediaDetailByIdEndpoint>();
        await ep.HandleAsync(request, CancellationToken);
        var result = ep.Response as ResultDTO<PlexMediaDTO>;

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();

        var validationResult = await PlexMediaDtoValidator.ValidateAsync(
            result.Value,
            TestContext.Current.CancellationToken
        );
        validationResult.Errors.ShouldBeEmpty();
        result.Value.Children.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldHavePlexMediaData_WhenValidMediaIdAndPlexMediaTypeTvShowIsRequested()
    {
        // Arrange
        await SetupDatabase(
            53442,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 10;
                config.TvShowSeasonCount = 3;
                config.TvShowEpisodeCount = 5;
            }
        );

        var testTvShow = IDbContext.PlexTvShows.FirstOrDefault(x => x.HasThumb);
        testTvShow.ShouldNotBeNull();

        var request = new GetMediaDetailByIdEndpointRequest(testTvShow.Id, PlexMediaType.TvShow);

        // Act
        var ep = SetupEndpointUnitTest<GetMediaDetailByIdEndpoint>();
        await ep.HandleAsync(request, CancellationToken);
        var result = ep.Response as ResultDTO<PlexMediaDTO>;

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();

        var validationResult = await PlexMediaDtoValidator.ValidateAsync(
            result.Value,
            TestContext.Current.CancellationToken
        );
        validationResult.Errors.ShouldBeEmpty();
        result.Value.Children.ShouldNotBeEmpty();
        foreach (var season in result.Value.Children)
        {
            var validationSeasonResult = await PlexMediaDtoValidator.ValidateAsync(
                season,
                TestContext.Current.CancellationToken
            );
            validationSeasonResult.Errors.ShouldBeEmpty();
            season.Children.ShouldNotBeEmpty();
            foreach (var episode in season.Children)
            {
                var validationEpisode = await PlexMediaDtoValidator.ValidateAsync(
                    episode,
                    TestContext.Current.CancellationToken
                );
                validationEpisode.Errors.ShouldBeEmpty();
                episode.Children.ShouldBeEmpty();
            }
        }
    }
}
