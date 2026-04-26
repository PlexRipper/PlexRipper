using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Reaparr.Application.UnitTests;

public class GetGitHubReleasesCommandUnitTests : BaseUnitTest<GetGitHubReleasesCommandHandler>
{
    [Test]
    public async Task ShouldReturnOnlyNewerDevReleases_WhenCurrentVersionIsDevRelease()
    {
        // Arrange
        var releases = new[]
        {
            CreateRelease("v0.38.0-dev.5", isPrerelease: true),
            CreateRelease("v0.38.0-dev.6", isPrerelease: true),
            CreateRelease("v0.38.0-dev.7", isPrerelease: true),
            CreateRelease("v0.38.0", isPrerelease: false),
        };
        var expectedVersions = new[] { "v0.38.0-dev.7" };
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(CreateGitHubHttpClient(releases))
            .Verifiable(Times.Once());

        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0-dev.6");

        // Act
        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(x => x.Version).ToList().ShouldBe(expectedVersions);
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldReturnOnlyNewerStableReleases_WhenCurrentVersionIsStable()
    {
        // Arrange
        var releases = new[]
        {
            CreateRelease("v0.38.0-dev.7", isPrerelease: true),
            CreateRelease("v0.38.0", isPrerelease: false),
            CreateRelease("v0.38.1-dev.1", isPrerelease: true),
            CreateRelease("v0.38.1", isPrerelease: false),
        };
        var expectedVersions = new[] { "v0.38.1" };
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(CreateGitHubHttpClient(releases))
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0");

        // Act
        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(x => x.Version).ToList().ShouldBe(expectedVersions);
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldExcludeSameVersionRelease_WhenCurrentVersionMatchesRelease()
    {
        // Arrange
        var releases = new[]
        {
            CreateRelease("v0.38.0-dev.5", isPrerelease: true),
            CreateRelease("v0.38.0-dev.6", isPrerelease: true),
        };
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(CreateGitHubHttpClient(releases))
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0-dev.6");

        // Act
        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldExcludeOlderReleases_WhenReleasesAreBehindCurrentVersion()
    {
        // Arrange
        var releases = new[]
        {
            CreateRelease("v0.37.0-dev.1", isPrerelease: true),
            CreateRelease("v0.37.0-dev.2", isPrerelease: true),
        };
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(CreateGitHubHttpClient(releases))
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0-dev.1");

        // Act
        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldReturnEmptyList_WhenNoReleasesExist()
    {
        // Arrange
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(CreateGitHubHttpClient([]))
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0-dev.1");

        // Act

        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldReturnFailure_WhenHttpRequestThrows()
    {
        // Arrange
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(
                new HttpClient(new ThrowingHttpMessageHandler()) { BaseAddress = new Uri("https://api.github.com/") }
            )
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0-dev.1");

        // Act

        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldSkipMalformedReleaseTags_WhenGitHubContainsNonSemverRelease()
    {
        // Arrange
        var releases = new[]
        {
            CreateRelease("dev-latest", isPrerelease: true),
            CreateRelease("v0.38.0-dev.7", isPrerelease: true),
        };
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(CreateGitHubHttpClient(releases))
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0-dev.6");

        // Act

        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(x => x.Version).ShouldBe(["v0.38.0-dev.7"]);
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public void ShouldSetGitHubAuthorizationHeader_WhenGitHubTokenIsConfigured()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?> { [Environment.EnvKeys.GitHubToken] = "test-github-token" }
        );
        var services = new ServiceCollection();

        // Act
        services.RegisterGitHubHttpClient();
        using var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        using var client = httpClientFactory.CreateClient(HttpClientModule.GitHubClientName);

        // Assert
        client.DefaultRequestHeaders.Authorization.ShouldNotBeNull();
        client.DefaultRequestHeaders.Authorization.Scheme.ShouldBe("Bearer");
        client.DefaultRequestHeaders.Authorization.Parameter.ShouldBe("test-github-token");
    }

    [Test]
    public void ShouldNotSetGitHubAuthorizationHeader_WhenGitHubTokenIsMissing()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?> { [Environment.EnvKeys.GitHubToken] = null }
        );
        var services = new ServiceCollection();

        // Act
        services.RegisterGitHubHttpClient();
        using var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        using var client = httpClientFactory.CreateClient(HttpClientModule.GitHubClientName);

        // Assert
        client.DefaultRequestHeaders.Authorization.ShouldBeNull();
    }

    [Test]
    public async Task ShouldMapReleaseNotesCorrectly_WhenDevReleaseIsReturned()
    {
        // Arrange
        var publishedAt = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc);
        var releases = new[]
        {
            new GitHubReleaseDTO
            {
                TagName = "v0.38.0-dev.7",
                Prerelease = true,
                Body = "Release notes body",
                PublishedAt = publishedAt,
            },
        };
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(CreateGitHubHttpClient(releases))
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0-dev.6");

        // Act

        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        var note = result.Value[0];
        note.Version.ShouldBe("v0.38.0-dev.7");
        note.Notes.ShouldBe("Release notes body");
        note.ReleaseDate.ShouldBe(publishedAt);
        note.IsDevRelease.ShouldBeTrue();
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldMapIsDevReleaseFalse_WhenStableReleaseIsReturned()
    {
        // Arrange
        var publishedAt = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc);
        var releases = new[]
        {
            new GitHubReleaseDTO
            {
                TagName = "v0.38.1",
                Prerelease = false,
                Body = "Stable release notes",
                PublishedAt = publishedAt,
            },
        };
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(CreateGitHubHttpClient(releases))
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0");

        // Act
        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        var note = result.Value[0];
        note.Version.ShouldBe("v0.38.1");
        note.IsDevRelease.ShouldBeFalse();
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldExcludeStableReleases_WhenCurrentVersionIsDevRelease()
    {
        // Arrange
        var releases = new[]
        {
            CreateRelease("v0.38.0-dev.7", isPrerelease: true),
            CreateRelease("v0.38.0", isPrerelease: false),
            CreateRelease("v0.39.0", isPrerelease: false),
        };
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(CreateGitHubHttpClient(releases))
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0-dev.6");

        // Act
        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(x => x.Version).ShouldBe(["v0.38.0-dev.7"]);
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldExcludeDevReleases_WhenCurrentVersionIsStable()
    {
        // Arrange
        var releases = new[]
        {
            CreateRelease("v0.38.1-dev.1", isPrerelease: true),
            CreateRelease("v0.38.1-dev.2", isPrerelease: true),
            CreateRelease("v0.38.1", isPrerelease: false),
            CreateRelease("v0.39.0-dev.1", isPrerelease: true),
        };
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(CreateGitHubHttpClient(releases))
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0");

        // Act
        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(x => x.Version).ShouldBe(["v0.38.1"]);
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldReturnMultipleNewerDevReleases_WhenSeveralExistAboveCurrentVersion()
    {
        // Arrange
        var releases = new[]
        {
            CreateRelease("v0.38.0-dev.5", isPrerelease: true),
            CreateRelease("v0.38.0-dev.6", isPrerelease: true),
            CreateRelease("v0.38.0-dev.7", isPrerelease: true),
            CreateRelease("v0.38.0-dev.8", isPrerelease: true),
        };
        var expectedVersions = new[] { "v0.38.0-dev.7", "v0.38.0-dev.8" };
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(CreateGitHubHttpClient(releases))
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0-dev.6");

        // Act
        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(x => x.Version).ToList().ShouldBe(expectedVersions);
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldReturnMultipleNewerStableReleases_WhenSeveralExistAboveCurrentVersion()
    {
        // Arrange
        var releases = new[]
        {
            CreateRelease("v0.38.0", isPrerelease: false),
            CreateRelease("v0.38.1", isPrerelease: false),
            CreateRelease("v0.39.0", isPrerelease: false),
        };
        var expectedVersions = new[] { "v0.38.1", "v0.39.0" };
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(CreateGitHubHttpClient(releases))
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0");

        // Act
        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(x => x.Version).ToList().ShouldBe(expectedVersions);
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    [Test]
    public async Task ShouldReturnEmptyList_WhenApiReturnsNull()
    {
        // Arrange
        var sut = Sut;

        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.GitHubClientName))
            .Returns(
                new HttpClient(new NullResponseHttpMessageHandler())
                {
                    BaseAddress = new Uri("https://api.github.com/"),
                }
            )
            .Verifiable(Times.Once());
        SetAppBuildInfo(x => x.InformationalVersion = "0.38.0-dev.1");

        // Act

        var result = await sut.ExecuteAsync(new GetGitHubReleasesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
        Mock.Mock<IHttpClientFactory>().Verify();
    }

    private static HttpClient CreateGitHubHttpClient(IReadOnlyList<GitHubReleaseDTO> releases) =>
        new(new GitHubReleasesResponseHandler(releases)) { BaseAddress = new Uri("https://api.github.com/") };

    private static GitHubReleaseDTO CreateRelease(string tagName, bool isPrerelease) =>
        new()
        {
            TagName = tagName,
            Prerelease = isPrerelease,
            PublishedAt = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
        };

    private sealed class GitHubReleasesResponseHandler(IReadOnlyList<GitHubReleaseDTO> releases) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StringContent(JsonSerializer.Serialize(releases), Encoding.UTF8, "application/json"),
                }
            );
        }
    }

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => throw new HttpRequestException("Simulated network failure");
    }

    private sealed class NullResponseHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StringContent("null", Encoding.UTF8, "application/json"),
                }
            );
    }
}
