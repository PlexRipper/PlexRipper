namespace Reaparr.Application.UnitTests;

public class GetMediaComparisonDetailsEndpointUnitTests
    : BaseEndpointUnitTest<GetMediaComparisonDetailsEndpoint, GetMediaComparisonDetailsEndpointRequest, ResultDTO<PlexMediaComparisonDetailsDTO>>
{
    [Test]
    public async Task ShouldDispatchMovieCommand_WhenMovieComparisonDetailsRequested()
    {
        // Arrange
        var request = new GetMediaComparisonDetailsEndpointRequest(1887, PlexMediaType.Movie);
        var commandResult = Result.Ok(new PlexMediaComparisonDetailsDTO
        {
            PlexMediaId = request.PlexMediaId,
            Type = PlexMediaType.Movie,
            State = PlexMediaComparisonState.Owned,
            Rows = [],
        });

        Mock.SetupCommand<Result<PlexMediaComparisonDetailsDTO>>(x =>
                (x as GetMovieMediaComparisonDetailsCommand) != null &&
                ((GetMovieMediaComparisonDetailsCommand)x).PlexMediaId == request.PlexMediaId)
            .ReturnsAsync(commandResult)
            .Verifiable(Times.Once);
        Mock.SetupCommand<Result<PlexMediaComparisonDetailsDTO>>(x => (x as GetTvShowMediaComparisonDetailsCommand) != null)
            .ReturnsAsync(commandResult)
            .Verifiable(Times.Never);

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Type.ShouldBe(PlexMediaType.Movie);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldDispatchTvShowCommand_WhenTvShowComparisonDetailsRequested()
    {
        // Arrange
        var request = new GetMediaComparisonDetailsEndpointRequest(1888, PlexMediaType.TvShow);
        var commandResult = Result.Ok(new PlexMediaComparisonDetailsDTO
        {
            PlexMediaId = request.PlexMediaId,
            Type = PlexMediaType.TvShow,
            State = PlexMediaComparisonState.Owned,
            Rows = [],
        });

        Mock.SetupCommand<Result<PlexMediaComparisonDetailsDTO>>(x => (x as GetMovieMediaComparisonDetailsCommand) != null)
            .ReturnsAsync(commandResult)
            .Verifiable(Times.Never);
        Mock.SetupCommand<Result<PlexMediaComparisonDetailsDTO>>(x =>
                (x as GetTvShowMediaComparisonDetailsCommand) != null &&
                ((GetTvShowMediaComparisonDetailsCommand)x).PlexMediaId == request.PlexMediaId)
            .ReturnsAsync(commandResult)
            .Verifiable(Times.Once);

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Type.ShouldBe(PlexMediaType.TvShow);
        Mock.Mock<ICommandExecutor>().Verify();
    }
}