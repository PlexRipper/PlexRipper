using System.Xml.Serialization;

namespace Reaparr.PublicAPI.UnitTests;

public class GetCapabilitiesCommandUnitTests : BaseCommandUnitTest<GetCapabilitiesCommand>
{
    [Test]
    public async Task ShouldSerializeOnlyStandardCapabilitySections()
    {
        // Arrange
        var command = new GetCapabilitiesCommand();
        // Act
        var result = await TestHandlerExecuteAsync<TorznabCapsResponseDTO>(command);
        var serializer = new XmlSerializer(typeof(TorznabCapsResponseDTO));
        await using var stream = new MemoryStream();
        serializer.Serialize(stream, result.Value);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var xml = await reader.ReadToEndAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        xml.ShouldContain("<server");
        xml.ShouldContain("<limits");
        xml.ShouldContain("<searching>");
        xml.ShouldContain("<categories>");
        result.Value.Categories.Select(x => x.Id).ShouldBe([
            2000, 2010, 2030, 2040, 2045, 2050, 2070,
            5000, 5020, 5030, 5040, 5045, 5060, 5070, 5080,
        ]);
        result.Value.Categories.Select(x => x.Name).ShouldBe([
            "Movies",
            "Movies/Foreign",
            "Movies/SD",
            "Movies/HD",
            "Movies/UHD",
            "Movies/BluRay",
            "Movies/WEBDL",
            "TV",
            "TV/Foreign",
            "TV/SD",
            "TV/HD",
            "TV/UHD",
            "TV/Sport",
            "TV/Anime",
            "TV/Documentary",
        ]);
        xml.ShouldNotContain("<torznab:attributes");
        xml.ShouldNotContain("<attributes");
    }
}
