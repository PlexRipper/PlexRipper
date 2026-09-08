namespace Reaparr.Application.UnitTests;

public class SonarrHttpClientFactoryUnitTests : BaseUnitTest<SonarrHttpClientFactory>
{
    [Test]
    public void ShouldCreateClientFromUnsavedValues()
    {
        Mock.Mock<IHttpClientFactory>()
            .Setup(x => x.CreateClient(HttpClientModule.DefaultClientName))
            .Returns(() => new HttpClient());
        var sut = Mock.Create<SonarrHttpClientFactory>();

        var result = sut.Create("http://sonarr.test:8989/", "sonarr-key");

        result.IsSuccess.ShouldBeTrue();
        using var client = result.Value;
        client.BaseAddress.ShouldBe(new Uri("http://sonarr.test:8989/"));
        client.DefaultRequestHeaders.GetValues("X-Api-Key").Single().ShouldBe("sonarr-key");
        client.DefaultRequestHeaders.Accept.Single().MediaType.ShouldBe("application/json");
    }

    [Test]
    public async Task ShouldReturnFailureWhenPersistedIntegrationDoesNotExist()
    {
        await SetupDatabase(626560);
        var sut = Mock.Create<SonarrHttpClientFactory>();

        var result = await sut.CreateAsync(Guid.NewGuid());

        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains(nameof(SonarrIntegration)));
    }
}
