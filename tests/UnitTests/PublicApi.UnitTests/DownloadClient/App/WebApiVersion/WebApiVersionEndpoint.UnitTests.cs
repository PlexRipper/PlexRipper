using System.Text;

namespace Reaparr.PublicAPI.UnitTests;

public class WebApiVersionEndpointUnitTests : BaseEndpointWithoutRequestUnitTest<WebApiVersionEndpoint, string>
{
    [Test]
    public async Task ShouldReturnWebApiVersionString_WhenBodyParsed()
    {
        // Arrange
        // Act
        var endpointResult = await TestEndpointHandleAsync();

        // Assert
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status200OK);

        var body = endpointResult.Response;
        body.ShouldNotBeNullOrWhiteSpace();

        // Sonarr/Radarr call Version.Parse() directly on the webapiVersion response body.
        // This must not throw; the string must be a valid dotted numeric version.
        var parsed = Version.Parse(body);
        parsed.ShouldNotBeNull();
    }
}
