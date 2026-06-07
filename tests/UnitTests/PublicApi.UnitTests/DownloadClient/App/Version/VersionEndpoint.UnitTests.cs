using System.Text;

namespace Reaparr.PublicAPI.UnitTests;

public class VersionEndpointUnitTests : BaseEndpointWithoutRequestUnitTest<VersionEndpoint, string>
{
    [Test]
    public async Task ShouldReturnVersionString_WhenPrefixTrimmed()
    {
        // Arrange
        // Act
        var endpointResult = await TestEndpointHandleAsync();

        // Assert
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status200OK);

        var body = endpointResult.Response;
        body.ShouldNotBeNullOrWhiteSpace();

        // Sonarr/Radarr parse the version by trimming the leading 'v' then reading as a string.
        // This should not throw a FormatException or OverflowException.
        var trimmed = body.TrimStart('v');
        var parsed = Version.Parse(trimmed);
        parsed.ShouldNotBeNull();

        // No persistence expected in this endpoint.
    }
}
