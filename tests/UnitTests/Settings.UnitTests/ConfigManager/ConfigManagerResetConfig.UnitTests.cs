using System.IO.Abstractions;
using Autofac;
using Reaparr.Environment;
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
        Mock.Mock<IUserSettings>().Setup(x => x.Reset());

        // Were mocking other methods from ConfigManager, that's why we need to mock it manually here
        var sut = new Mock<ConfigManager>(
            MockBehavior.Strict,
            Mock.Container.Resolve<ILogger>(),
            Mock.Container.Resolve<IPathProvider>(),
            Mock.Container.Resolve<IUserSettings>(),
            Mock.Container.Resolve<IFile>(),
            Mock.Container.Resolve<IPath>(),
            Mock.Container.Resolve<IDirectory>()
        );
        sut.Setup(x => x.SaveConfig()).Returns(Result.Ok);

        // Since ResetConfig is virtual we need to callBase here
        sut.Setup(x => x.ResetConfig()).CallBase();

        // Act
        var resetResult = sut.Object.ResetConfig();

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
        sut.Verify(x => x.SaveConfig(), Times.Once);
        Mock.Mock<IUserSettings>().Verify(x => x.Reset(), Times.Once);
    }
}
