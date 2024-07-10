using Application.Contracts;
using Application.Contracts.Validators;
using Data.Contracts;
using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;

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
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.MovieCount = movieCount;
        });
        var ep = Factory.Create<GetPlexLibraryMediaEndpoint>(ctx =>
            ctx.AddTestServices(s => s.AddTransient(_ => mock.Create<IPlexRipperDbContext>()))
        );

        var request = new GetPlexLibraryMediaEndpointRequest()
        {
            PlexLibraryId = 1,
            Page = 0,
            Size = 0,
        };

        // Act
        await ep.HandleAsync(request, default);
        var result = ep.Response as ResultDTO<List<PlexMediaSlimDTO>>;

        // Assert
        result.ShouldNotBeNull();
        result.Value.Count.ShouldBe(movieCount);
        foreach (var plexMediaSlimDto in result.Value)
        {
            var validationResult = await PlexMediaSlimDtoValidator.ValidateAsync(plexMediaSlimDto);
            validationResult.Errors.ShouldBeEmpty();
            plexMediaSlimDto.Children.ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task ShouldHaveAllThePlexLibraryMedia_WhenPageAndSizeAreNotSetAndMediaIsTvShows()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.TvShowCount = 100;
            config.TvShowSeasonCount = 3;
            config.TvShowEpisodeCount = 5;
        });

        var ep = Factory.Create<GetPlexLibraryMediaEndpoint>(ctx =>
            ctx.AddTestServices(s => s.AddTransient(_ => mock.Create<IPlexRipperDbContext>()))
        );

        var request = new GetPlexLibraryMediaEndpointRequest()
        {
            PlexLibraryId = 1,
            Page = 0,
            Size = 0,
        };

        // Act
        await ep.HandleAsync(request, default);
        var result = ep.Response as ResultDTO<List<PlexMediaSlimDTO>>;

        // Assert
        result.ShouldNotBeNull();
        result.Value.Count.ShouldBe(100);
        foreach (var plexMediaSlimDto in result.Value)
        {
            var validationResult = await PlexMediaSlimDtoValidator.ValidateAsync(plexMediaSlimDto);
            validationResult.Errors.ShouldBeEmpty();
            plexMediaSlimDto.Children.ShouldBeEmpty();
        }
    }
}
