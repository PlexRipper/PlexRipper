namespace Reaparr.Application.UnitTests;

public class ValidateFolderPathsCommandUnitTests : BaseCommandUnitTest<ValidateFolderPathsCommand>
{
    [Test]
    public async Task ShouldReturnFailure_WhenDefaultDownloadFolderDoesNotExist()
    {
        // Arrange
        await SetupDatabase(59901);
        var dbContext = IDbContext;
        var downloadFolderPath = await dbContext.FolderPaths.AsTracking().SingleAsync(
            x => x.Id == PlexMediaType.None.ToDefaultDestinationFolderId(),
            CancellationToken
        );
        downloadFolderPath.DirectoryPath = @"D:\Downloads";
        await dbContext.SaveChangesAsync(CancellationToken);
        var folderPathCount = await dbContext.FolderPaths.CountAsync(CancellationToken);

        Mock.Mock<IDirectory>()
            .Setup(x => x.Exists(It.IsAny<string>()))
            .Returns<string>(path => path != downloadFolderPath.DirectoryPath)
            .Verifiable(Times.Exactly(folderPathCount));

        // Act
        var result = await TestHandlerExecuteAsync(new ValidateFolderPathsCommand());

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].Message.ShouldContain(downloadFolderPath.DisplayName);
        Mock.Mock<IDirectory>().Verify();
    }

    [Test]
    public async Task ShouldReturnSuccess_WhenSetupPathsExistAcrossSupportedPathStyles()
    {
        // Arrange
        await SetupDatabase(59902);
        var dbContext = IDbContext;
        var folderPaths = await dbContext.FolderPaths.AsTracking().OrderBy(x => x.Id).ToListAsync(CancellationToken);

        var windowsDrivePath = @"D:\Downloads";
        var linuxMountPath = "/mnt/plex/g/Films";
        var uncPath = @"\\server\share\Series";

        folderPaths.Single(x => x.Id == PlexMediaType.None.ToDefaultDestinationFolderId()).DirectoryPath = windowsDrivePath;
        folderPaths.Single(x => x.Id == PlexMediaType.Movie.ToDefaultDestinationFolderId()).DirectoryPath = linuxMountPath;
        folderPaths.Single(x => x.Id == PlexMediaType.TvShow.ToDefaultDestinationFolderId()).DirectoryPath = uncPath;
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDirectory>()
            .Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true)
            .Verifiable(Times.Exactly(folderPaths.Count));

        // Act
        var result = await TestHandlerExecuteAsync(new ValidateFolderPathsCommand());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<IDirectory>().Verify(x => x.Exists(windowsDrivePath), Times.Once);
        Mock.Mock<IDirectory>().Verify(x => x.Exists(linuxMountPath), Times.Once);
        Mock.Mock<IDirectory>().Verify(x => x.Exists(uncPath), Times.Once);
        Mock.Mock<IDirectory>().Verify();
    }
}
