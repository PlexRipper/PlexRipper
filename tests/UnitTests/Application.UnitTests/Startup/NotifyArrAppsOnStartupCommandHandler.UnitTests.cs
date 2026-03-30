using System.Net;
using System.Net.Http;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Reaparr.Identity.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application.UnitTests;

public class NotifyArrAppsOnStartupCommandHandlerUnitTests : BaseUnitTest<NotifyArrAppsOnStartupCommandHandler>
{
    public NotifyArrAppsOnStartupCommandHandlerUnitTests()
        : base() { }

    // IRadarrSettings and ISonarrSettings inherit IBaseSettingsModule<T> which has a static abstract
    // member, making them incompatible with Moq. Inject concrete instances via TypedParameter instead.

    private NotifyArrAppsOnStartupCommandHandler CreateSut(
        RadarrSettings radarrSettings,
        SonarrSettings sonarrSettings
    ) =>
        Mock.Create<NotifyArrAppsOnStartupCommandHandler>(
            new TypedParameter(typeof(IRadarrSettings), radarrSettings),
            new TypedParameter(typeof(ISonarrSettings), sonarrSettings)
        );

    private static RadarrSettings ValidRadarrSettings(
        string baseUrl = "http://localhost:7878",
        string apiKey = "some-radarr-key"
    ) =>
        new()
        {
            IsConfigured = true,
            RadarrBaseUrl = baseUrl,
            RadarrApiKey = apiKey,
        };

    private static RadarrSettings NotConfiguredRadarrSettings() =>
        new()
        {
            IsConfigured = false,
            RadarrBaseUrl = string.Empty,
            RadarrApiKey = string.Empty,
        };

    private static SonarrSettings ValidSonarrSettings(
        string baseUrl = "http://localhost:8989",
        string apiKey = "some-sonarr-key"
    ) =>
        new()
        {
            IsConfigured = true,
            SonarrBaseUrl = baseUrl,
            SonarrApiKey = apiKey,
        };

    private static SonarrSettings NotConfiguredSonarrSettings() =>
        new()
        {
            IsConfigured = false,
            SonarrBaseUrl = string.Empty,
            SonarrApiKey = string.Empty,
        };

    private static NotifyArrAppsOnStartupCommand InstantCommand => new();

    private StatusCodeHandler SetupRadarrClient(HttpStatusCode statusCode)
    {
        var handler = new StatusCodeHandler(statusCode);
        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.RadarrClientName))
            .Returns(new HttpClient(handler) { BaseAddress = new Uri("http://radarr.test") })
            .Verifiable(Times.Once());
        return handler;
    }

    private StatusCodeHandler SetupSonarrClient(HttpStatusCode statusCode)
    {
        var handler = new StatusCodeHandler(statusCode);
        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.SonarrClientName))
            .Returns(new HttpClient(handler) { BaseAddress = new Uri("http://sonarr.test") })
            .Verifiable(Times.Once());
        return handler;
    }

    [Test]
    public async Task ShouldClearAllSessions_WhenCommandExecutes()
    {
        // Arrange
        await SetupDatabase(10001);
        var authContext = IAuthDbContext;
        authContext.DownloadClientSessions.AddRange(
            new DownloadClientSession
            {
                Sid = "sid-1",
                Username = "user",
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            },
            new DownloadClientSession
            {
                Sid = "sid-2",
                Username = "user",
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            }
        );
        await authContext.SaveChangesAsync(CancellationToken);

        var sut = CreateSut(NotConfiguredRadarrSettings(), NotConfiguredSonarrSettings());

        // Act
        var result = await sut.ExecuteAsync(InstantCommand, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var remaining = await IAuthDbContext.DownloadClientSessions.CountAsync(CancellationToken);
        remaining.ShouldBe(0);
    }

    [Test]
    public async Task ShouldReturnOk_WhenNeitherArrAppIsConfigured()
    {
        // Arrange
        await SetupDatabase(10002);

        var sut = CreateSut(NotConfiguredRadarrSettings(), NotConfiguredSonarrSettings());

        // Act
        var result = await sut.ExecuteAsync(InstantCommand, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        // No IHttpClientFactory calls — strict mock would throw if CreateClient() were invoked
    }

    [Test]
    public async Task ShouldPostToRadarrTestAll_WhenRadarrIsConfigured()
    {
        // Arrange
        await SetupDatabase(10003);

        var radarrHandler = SetupRadarrClient(HttpStatusCode.OK);
        var sut = CreateSut(ValidRadarrSettings(), NotConfiguredSonarrSettings());

        // Act
        var result = await sut.ExecuteAsync(InstantCommand, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        radarrHandler.RequestCount.ShouldBe(1);
        radarrHandler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        radarrHandler.LastRequest.RequestUri!.AbsolutePath.ShouldBe("/api/v3/downloadclient/testall");
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldPostToSonarrTestAll_WhenSonarrIsConfigured()
    {
        // Arrange
        await SetupDatabase(10004);

        var sonarrHandler = SetupSonarrClient(HttpStatusCode.OK);
        var sut = CreateSut(NotConfiguredRadarrSettings(), ValidSonarrSettings());

        // Act
        var result = await sut.ExecuteAsync(InstantCommand, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        sonarrHandler.RequestCount.ShouldBe(1);
        sonarrHandler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        sonarrHandler.LastRequest.RequestUri!.AbsolutePath.ShouldBe("/api/v3/downloadclient/testall");
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldPostToBothTestAll_WhenBothAppsAreConfigured()
    {
        // Arrange
        await SetupDatabase(10005);

        var radarrHandler = SetupRadarrClient(HttpStatusCode.OK);
        var sonarrHandler = SetupSonarrClient(HttpStatusCode.OK);
        var sut = CreateSut(ValidRadarrSettings(), ValidSonarrSettings());

        // Act
        var result = await sut.ExecuteAsync(InstantCommand, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        radarrHandler.RequestCount.ShouldBe(1);
        sonarrHandler.RequestCount.ShouldBe(1);
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldReturnOk_WhenRadarrTestAllReturns400()
    {
        // Arrange
        await SetupDatabase(10006);

        // 400 is expected when some download clients fail their test — still clears DisabledTill.
        var radarrHandler = SetupRadarrClient(HttpStatusCode.BadRequest);
        var sut = CreateSut(ValidRadarrSettings(), NotConfiguredSonarrSettings());

        // Act
        var result = await sut.ExecuteAsync(InstantCommand, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        radarrHandler.RequestCount.ShouldBe(1);
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldReturnOk_WhenRadarrTestAllReturnsServerError()
    {
        // Arrange
        await SetupDatabase(10007);

        // A 503 means Radarr is unreachable — failure is non-fatal, startup continues.
        var radarrHandler = SetupRadarrClient(HttpStatusCode.ServiceUnavailable);
        var sut = CreateSut(ValidRadarrSettings(), NotConfiguredSonarrSettings());

        // Act
        var result = await sut.ExecuteAsync(InstantCommand, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        radarrHandler.RequestCount.ShouldBe(1);
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldNotCallRadarr_WhenRadarrUrlIsInvalid()
    {
        // Arrange
        await SetupDatabase(10008);

        var sut = CreateSut(ValidRadarrSettings(baseUrl: string.Empty), NotConfiguredSonarrSettings());

        // Act
        var result = await sut.ExecuteAsync(InstantCommand, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        // No CreateClient(RadarrClientName) call — strict mock would throw if it were made
    }

    [Test]
    public async Task ShouldNotCallSonarr_WhenSonarrApiKeyIsInvalid()
    {
        // Arrange
        await SetupDatabase(10009);

        var sut = CreateSut(NotConfiguredRadarrSettings(), ValidSonarrSettings(apiKey: string.Empty));

        // Act
        var result = await sut.ExecuteAsync(InstantCommand, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        // No CreateClient(SonarrClientName) call — strict mock would throw if it were made
    }

    private sealed class StatusCodeHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            RequestCount++;
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(statusCode) { RequestMessage = request });
        }
    }
}
