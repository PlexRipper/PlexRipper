using Reaparr.Environment;

namespace Reaparr.AppHost.UnitTests;

public class DesktopWindowUnitTests : BaseUnitTest<DesktopWindow>
{
    [Test]
    public void ShouldConstructWindowWithoutThrowing_WhenValidUriAndBuildInfoProvided()
    {
        // Arrange
        var appBuildInfo = new MockAppBuildInfo { CurrentOS = OperatingSystemPlatform.Windows };

        // Act
        var action = () => new DesktopWindow(new Uri("http://localhost:5000"), appBuildInfo);

        // Assert
        action.ShouldNotThrow();
    }
}
