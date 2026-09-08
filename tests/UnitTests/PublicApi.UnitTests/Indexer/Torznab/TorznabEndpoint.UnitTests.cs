using Reaparr.Application.Contracts;

namespace Reaparr.PublicAPI.UnitTests;

public class TorznabEndpointUnitTests : BaseEndpointUnitTest<TorznabEndpoint, TorznabEndpointRequest>
{
    [Test]
    public async Task ShouldRunOnlyRequestedTvSearch_WhenGenericSearchCategoryIsTv()
    {
        // Arrange
        await SetupDatabase(
            6521,
            config =>
            {
                config.RadarrIntegrationCount = 1;
            }
        );

        var integration = (await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken)).Id.ToRadarrIdentity();
        var request = new TorznabEndpointRequest
        {
            Type = "search",
            Query = "Silo",
            Categories = [5030],
            Limit = 10,
            Offset = 2,
            ApiKey = "generic-key",
        };

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<SearchTvShowCommand>(command =>
                        command.Query == request.Query
                        && command.Limit == request.Limit
                        && command.Offset == request.Offset
                        && command.Integration == integration
                        && command.TorznabApiKey == request.ApiKey
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(SearchResult("tv-result"))
            .Verifiable(Times.Once());

        // Act
        var endpointResult = await TestEndpointHandleAsync(request, integrationIdentity: integration);

        // Assert
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        var responseBody = endpointResult.Endpoint.HttpContext.Response.Body;
        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody, leaveOpen: true);
        var xml = await reader.ReadToEndAsync();
        xml.ShouldContain("tv-result");
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<SearchMovieCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldMergeAndLimitResults_WhenGenericSearchHasNoCategories()
    {
        // Arrange
        await SetupDatabase(
            6522,
            config =>
            {
                config.RadarrIntegrationCount = 1;
            }
        );

        var integration = (await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken)).Id.ToRadarrIdentity();
        var request = new TorznabEndpointRequest
        {
            Type = "search",
            Query = "Dune",
            Limit = 2,
            Offset = 1,
            ApiKey = "generic-key",
        };

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<SearchTvShowCommand>(command =>
                        command.Query == request.Query
                        && command.Limit == request.Limit
                        && command.Offset == request.Offset
                        && command.Integration == integration
                        && command.TorznabApiKey == request.ApiKey
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(SearchResult("tv-result"))
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<SearchMovieCommand>(command =>
                        command.Query == request.Query
                        && command.Limit == request.Limit
                        && command.Offset == request.Offset
                        && command.Integration == integration
                        && command.TorznabApiKey == request.ApiKey
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(SearchResult("movie-result-1", "movie-result-2"))
            .Verifiable(Times.Once());

        // Act
        var endpointResult = await TestEndpointHandleAsync(request, integrationIdentity: integration);

        // Assert
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        var responseBody = endpointResult.Endpoint.HttpContext.Response.Body;
        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody, leaveOpen: true);
        var xml = await reader.ReadToEndAsync();
        xml.ShouldContain("tv-result");
        xml.ShouldContain("movie-result-1");
        xml.ShouldNotContain("movie-result-2");
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnSuccess_WhenOneGenericSearchFailsAfterAnotherReturnsNoItems()
    {
        // Arrange
        await SetupDatabase(
            6523,
            config =>
            {
                config.RadarrIntegrationCount = 1;
            }
        );

        var integration = (await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken)).Id.ToRadarrIdentity();
        var request = new TorznabEndpointRequest
        {
            Type = "search",
            Query = "Missing",
            Limit = 10,
            Offset = 0,
            ApiKey = "generic-key",
        };

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SearchTvShowCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SearchResult())
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SearchMovieCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<TorznabMediaSearchResponseDTO>("movie search failed"))
            .Verifiable(Times.Once());

        // Act
        var endpointResult = await TestEndpointHandleAsync(request, integrationIdentity: integration);

        // Assert
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        var responseBody = endpointResult.Endpoint.HttpContext.Response.Body;
        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody, leaveOpen: true);
        var xml = await reader.ReadToEndAsync();
        xml.ShouldContain("<channel");
        xml.ShouldNotContain("movie search failed");
        Mock.Mock<ICommandExecutor>().Verify();
    }

    private static Result<TorznabMediaSearchResponseDTO> SearchResult(params string[] titles) =>
        Result.Ok(
            new TorznabMediaSearchResponseDTO
            {
                Channel = new TorznabChannel { Items = [.. titles.Select(title => new TorznabItem { Title = title })] },
            }
        );
}
