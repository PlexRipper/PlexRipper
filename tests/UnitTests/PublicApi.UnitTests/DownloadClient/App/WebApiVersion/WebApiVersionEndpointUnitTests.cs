using System.Text;
using Reaparr.PublicAPI;

namespace PublicApi.UnitTests;

public class WebApiVersionEndpointUnitTests : BaseUnitTest
{
    public WebApiVersionEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnWebApiVersionString_ThatCanBeParsedAsVersion()
    {
        // Arrange
        var ep = SetupEndpointUnitTest<WebApiVersionEndpoint>();
        var buffer = new MemoryStream();
        ep.HttpContext.Response.Body = buffer;

        // Act
        await ep.HandleAsync(CancellationToken);

        // Assert
        ep.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);

        var body = Encoding.UTF8.GetString(buffer.ToArray());
        body.ShouldNotBeNullOrWhiteSpace();

        // Sonarr/Radarr call Version.Parse() directly on the webapiVersion response body.
        // This must not throw; the string must be a valid dotted numeric version.
        var parsed = Version.Parse(body);
        parsed.ShouldNotBeNull();
    }
}
