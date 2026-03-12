using System.Net;
using System.Net.Http;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class GetDirectDownloadUrlCommandUnitTests : BaseUnitTest<GetDirectDownloadUrlCommandHandler>
{
    public GetDirectDownloadUrlCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnUrlWithoutDownloadQuery_WhenInitialProbeSucceeds()
    {
        // Arrange
        await SetupDatabase(
            90201,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var downloadTask = await IDbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        SetupHttpClientFactory(HttpStatusCode.OK);

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(
            new GetDirectDownloadUrlCommand(downloadTask.PlexServerId, downloadTask.FileLocationUrl),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotContain("download=1");
        Mock.Mock<IHttpClientFactory>().Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task ShouldAppendDownloadQuery_WhenInitialProbeIsForbidden()
    {
        // Arrange
        await SetupDatabase(
            90202,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var downloadTask = await IDbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        SetupHttpClientFactory(HttpStatusCode.Forbidden, HttpStatusCode.OK);

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(
            new GetDirectDownloadUrlCommand(downloadTask.PlexServerId, downloadTask.FileLocationUrl),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldContain("download=1");
        Mock.Mock<IHttpClientFactory>().Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenForbiddenAndFallbackAlsoFails()
    {
        // Arrange
        await SetupDatabase(
            90203,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var downloadTask = await IDbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        SetupHttpClientFactory(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(
            new GetDirectDownloadUrlCommand(downloadTask.PlexServerId, downloadTask.FileLocationUrl),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IHttpClientFactory>().Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once());
    }

    private GetDirectDownloadUrlCommandHandler CreateSut() =>
        Mock.Create<GetDirectDownloadUrlCommandHandler>(new TypedParameter(typeof(IReaparrDbContext), IDbContext));

    private void SetupHttpClientFactory(params HttpStatusCode[] statuses)
    {
        var httpClient = new HttpClient(new SequenceStatusCodeHandler(statuses));
        Mock.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
    }

    private sealed class SequenceStatusCodeHandler(params HttpStatusCode[] statuses) : HttpMessageHandler
    {
        private readonly Queue<HttpStatusCode> _statuses = new(statuses);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            if (_statuses.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(SequenceStatusCodeHandler)} was exhausted while probing {request.RequestUri}."
                );
            }

            var statusCode = _statuses.Dequeue();
            return Task.FromResult(new HttpResponseMessage(statusCode) { RequestMessage = request });
        }
    }
}
