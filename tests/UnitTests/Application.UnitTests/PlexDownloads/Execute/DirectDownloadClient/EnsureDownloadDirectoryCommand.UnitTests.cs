using ByteSizeLib;

namespace Reaparr.Application.UnitTests;

public class EnsureDownloadDirectoryCommandUnitTests : BaseCommandUnitTest<EnsureDownloadDirectoryCommand>
{
    // -------------------------------------------------------------------------
    // Validator tests
    // -------------------------------------------------------------------------

    [Test]
    public async Task ShouldReturnFailedResult_WhenDirectoryIsEmpty()
    {
        // Arrange
        var command = new EnsureDownloadDirectoryCommand(string.Empty, 1024);

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("Directory cannot be empty"));
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenFileSizeIsZero()
    {
        // Arrange
        var command = new EnsureDownloadDirectoryCommand("/downloads/reaparr/Movies/Test", 0);

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("File size must be greater than zero"));
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenFileSizeIsNegative()
    {
        // Arrange
        var command = new EnsureDownloadDirectoryCommand("/downloads/reaparr/Movies/Test", -1);

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("File size must be greater than zero"));
    }

    // -------------------------------------------------------------------------
    // Handler tests
    // -------------------------------------------------------------------------

    [Test]
    public async Task ShouldReturnFailedResult_WhenCreateDirectoryThrows()
    {
        // Arrange
        const string directory = "/downloads/reaparr/Movies/Test Movie (2024)";
        var command = new EnsureDownloadDirectoryCommand(directory, 1_000_000);

        SetupFileSystem(fs => fs.AddFile(directory, new MockFileData([])));

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenDiskSpaceCheckFails()
    {
        // Arrange — use a Windows-style path that MockFileSystem on Linux cannot resolve to a drive,
        // causing DriveInfo.New() to throw and GetAvailableSpaceByDirectory to return a failed Result.
        const string directory = @"C:\no-such-drive\Movies";
        var command = new EnsureDownloadDirectoryCommand(directory, 1_000_000);

        SetupFileSystem(fs =>
        {
            fs.AddDrive(
                @"D:\",
                new MockDriveData
                {
                    IsReady = true,
                    DriveType = DriveType.Fixed,
                    AvailableFreeSpace = DefaultAvailableSpace,
                }
            );
        });

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenAvailableSpaceIsLessThanFileSize()
    {
        // Arrange — configure the drive with less free space than the requested file size
        const string directory = "/downloads/reaparr/Movies/Test Movie (2024)";
        const long fileSize = ByteSize.BytesInMegaByte * 500; // 500 MB
        const long availableSpace = ByteSize.BytesInMegaByte * 100; // 100 MB — not enough

        var command = new EnsureDownloadDirectoryCommand(directory, fileSize);

        SetupFileSystem(fs =>
        {
            fs.AddDrive(
                "/",
                new MockDriveData
                {
                    IsReady = true,
                    DriveType = DriveType.Fixed,
                    AvailableFreeSpace = availableSpace,
                }
            );
        });

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("not enough space") && e.Message.Contains(directory));
    }

    [Test]
    public async Task ShouldReturnSuccessResult_WhenDirectoryIsCreatedAndSpaceIsSufficient()
    {
        // Arrange
        const string directory = "/downloads/reaparr/Movies/Test Movie (2024)";
        const long fileSize = ByteSize.BytesInMegaByte * 500; // 500 MB

        var command = new EnsureDownloadDirectoryCommand(directory, fileSize);

        SetupFileSystem(); // DefaultAvailableSpace is 1000 GB — well above 500 MB

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Container.Resolve<IFileSystem>().Directory.Exists(directory).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnSuccessResult_WhenAvailableSpaceExactlyEqualsFileSize()
    {
        // Arrange
        const string directory = "/downloads/reaparr/Movies/Test Movie (2024)";
        const long fileSize = ByteSize.BytesInMegaByte * 500; // 500 MB

        var command = new EnsureDownloadDirectoryCommand(directory, fileSize);

        SetupFileSystem(fs =>
        {
            fs.AddDrive(
                "/",
                new MockDriveData
                {
                    IsReady = true,
                    DriveType = DriveType.Fixed,
                    AvailableFreeSpace = fileSize, // exactly equal — should succeed
                }
            );
        });

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Container.Resolve<IFileSystem>().Directory.Exists(directory).ShouldBeTrue();
    }
}
