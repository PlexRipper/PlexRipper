namespace PlexRipper.BaseTests;

public interface IMockPlexApiServer
{
    void Setup(Mock<HttpMessageHandler> handler, Action<PlexApiDataConfig> options);
}
