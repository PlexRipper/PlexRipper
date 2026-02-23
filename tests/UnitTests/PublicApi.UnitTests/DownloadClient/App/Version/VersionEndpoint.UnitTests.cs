using System.Text;
using Reaparr.Data.Contracts;
using Reaparr.PublicAPI;

namespace PublicApi.UnitTests;

public class VersionEndpointUnitTests : BaseUnitTest<VersionEndpoint>
{
    public VersionEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnVersionString_WhenPrefixTrimmed()
    {
        // Arrange
        var ep = SetupEndpointUnitTest<VersionEndpoint>();
        var buffer = new MemoryStream();
        ep.HttpContext.Response.Body = buffer;

        // Act
        await ep.HandleAsync(CancellationToken);

        // Assert
        ep.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);

        var body = Encoding.UTF8.GetString(buffer.ToArray());
        body.ShouldNotBeNullOrWhiteSpace();

        // Sonarr/Radarr parse the version by trimming the leading 'v' then reading as a string.
        // This should not throw a FormatException or OverflowException.
        var trimmed = body.TrimStart('v');
        var parsed = Version.Parse(trimmed);
        parsed.ShouldNotBeNull();

        Mock.Mock<IReaparrDbContext>()
            .Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
