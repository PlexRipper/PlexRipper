namespace Reaparr.Application.UnitTests;

public class RadarrHttpClientFactoryUnitTests : BaseUnitTest<RadarrHttpClientFactory>
{
    [Test]
    public void ShouldCreateClientFromUnsavedValues()
    {
        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.DefaultClientName))
            .Returns(() => new HttpClient());
        var sut = Mock.Create<RadarrHttpClientFactory>();

        var result = sut.Create("http://radarr.test:7878/", "radarr-key");

        result.IsSuccess.ShouldBeTrue();
        using var client = result.Value;
        client.BaseAddress.ShouldBe(new Uri("http://radarr.test:7878/"));
        client.DefaultRequestHeaders.GetValues("X-Api-Key").Single().ShouldBe("radarr-key");
        client.DefaultRequestHeaders.Accept.Single().MediaType.ShouldBe("application/json");
    }

    [Test]
    public async Task ShouldReturnFailureWhenPersistedIntegrationDoesNotExist()
    {
        await SetupDatabase(626561);
        var sut = Mock.Create<RadarrHttpClientFactory>();

        var result = await sut.CreateAsync(Guid.NewGuid(), CancellationToken);

        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message == "The Radarr integration was not found.");
    }
}
