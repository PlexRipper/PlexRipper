namespace Reaparr.Application.UnitTests;

public class IntegrationApiKeyGeneratorUnitTests
{
    [Test]
    public void ShouldGenerateQBittorrentApiKeyInExactFormat()
    {
        var apiKey = IntegrationApiKeyGenerator.GenerateQBittorrentApiKey();

        apiKey.Length.ShouldBe(32);
        apiKey.ShouldStartWith("qbt_");
        apiKey[4..]
            .ShouldAllBe(character => "23456789ABCDEFGHIJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz".Contains(character));
    }

    [Test]
    public void ShouldGenerateIndependentTorznabApiKeyInExactFormat()
    {
        var apiKey = IntegrationApiKeyGenerator.GenerateTorznabApiKey();

        apiKey.Length.ShouldBe(32);
        apiKey.ShouldAllBe(character => "0123456789abcdef".Contains(character));
    }

    [Test]
    public void ShouldGenerateUniqueKeys()
    {
        var qbittorrentKeys = Enumerable
            .Range(0, 100)
            .Select(_ => IntegrationApiKeyGenerator.GenerateQBittorrentApiKey());
        var torznabKeys = Enumerable.Range(0, 100).Select(_ => IntegrationApiKeyGenerator.GenerateTorznabApiKey());

        qbittorrentKeys.Distinct().Count().ShouldBe(100);
        torznabKeys.Distinct().Count().ShouldBe(100);
    }
}
