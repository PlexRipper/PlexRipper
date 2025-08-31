using System.IO.Abstractions;
using Autofac;
using Reaparr.Environment;
using Reaparr.Logging;
using Reaparr.Settings.Contracts;

namespace Reaparr.Settings.UnitTests;

public class ConfigManagerResetConfigUnitTests : BaseUnitTest<ConfigManager>
{
    public ConfigManagerResetConfigUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldReturnOkResult_WhenSettingsAreReset()
    {
        // Arrange
        mock.Mock<IUserSettings>().Setup(x => x.Reset());

        // Were mocking other methods from ConfigManager, that's why we need to mock it manually here
        var sut = new Mock<ConfigManager>(
            MockBehavior.Strict,
            mock.Container.Resolve<ILog>(),
            mock.Container.Resolve<IPathProvider>(),
            mock.Container.Resolve<IUserSettings>(),
            mock.Container.Resolve<IFile>(),
            mock.Container.Resolve<IPath>(),
            mock.Container.Resolve<IDirectory>()
        );
        sut.Setup(x => x.SaveConfig()).Returns(Result.Ok);

        // Since ResetConfig is virtual we need to callBase here
        sut.Setup(x => x.ResetConfig()).CallBase();

        // Act
        var resetResult = sut.Object.ResetConfig();

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
        sut.Verify(x => x.SaveConfig(), Times.Once);
        mock.Mock<IUserSettings>().Verify(x => x.Reset(), Times.Once);
    }
}
