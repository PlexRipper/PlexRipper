using Data.Contracts;
using Environment;
using FileSystem.Contracts;
using PlexRipper.Data;

namespace Domain.UnitTests;

public class PlexRipperDbContextManager_UnitTests : BaseUnitTest<PlexRipperDbContextManager>
{
    public PlexRipperDbContextManager_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldConnectToDatabaseAndCheckToMigrate_WhenDatabaseAlreadyExists()
    {
        // Arrange

        mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseName).Returns(() => PathProvider.DatabaseName);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => "/Config/" + PathProvider.DatabaseName);
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(() => "/TEST_PlexRipperSettings.json");
        mock.Mock<IFileSystem>().Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.CanConnect(), Times.Once);
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.GetPendingMigrations(), Times.Once);
    }
}
