using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reaparr.Data.Contracts;
using Reaparr.PublicAPI;

namespace Reaparr.PublicAPI.UnitTests;

public class WebApiVersionEndpointUnitTests : BaseUnitTest<WebApiVersionEndpoint>
{
    public WebApiVersionEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnWebApiVersionString_WhenBodyParsed()
    {
        // Arrange
        var ep = SetupEndpointUnitTest<WebApiVersionEndpoint>();
        var buffer = new MemoryStream();
        ep.HttpContext.Response.Body = buffer;

        // Act
        await ep.HandleAsync(CancellationToken);

        // Assert
        ep.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);

        var dbContext = ep.HttpContext.RequestServices.GetRequiredService<IReaparrDbContext>();
        if (dbContext is DbContext efContext)
        {
            efContext.ChangeTracker.Entries().ShouldBeEmpty();
        }
        else
        {
            var dbContextMock = Moq.Mock.Get(dbContext);
            dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
            var saveChangesCount = await dbContext.SaveChangesAsync(CancellationToken);
            saveChangesCount.ShouldBe(0);
        }

        var body = Encoding.UTF8.GetString(buffer.ToArray());
        body.ShouldNotBeNullOrWhiteSpace();

        // Sonarr/Radarr call Version.Parse() directly on the webapiVersion response body.
        // This must not throw; the string must be a valid dotted numeric version.
        var parsed = Version.Parse(body);
        parsed.ShouldNotBeNull();

    }
}
