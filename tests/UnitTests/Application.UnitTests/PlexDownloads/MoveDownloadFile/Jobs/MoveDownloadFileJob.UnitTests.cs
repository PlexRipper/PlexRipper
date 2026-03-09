using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class MoveDownloadFileJobUnitTests : BaseUnitTest<MoveDownloadFileJob>
{
    public MoveDownloadFileJobUnitTests(ITestOutputHelper output)
        : base(output) { }

    private IJobExecutionContext SetupJobContext(DownloadTaskKey key)
    {
        IDictionary<string, object> dict = new Dictionary<string, object>
        {
            { MoveDownloadFileJob.DownloadTaskIdParameter, JsonSerializer.Serialize(key) },
        };
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail.JobDataMap).Returns(new JobDataMap(dict));
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.CancellationToken).Returns(CancellationToken);
        return Mock.Create<IJobExecutionContext>();
    }

    [Fact]
    public async Task ShouldSetStatusToCompleted_WhenMoveSucceeds()
    {
        // Arrange
        await SetupDatabase(
            11001,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.AsTracking().FirstAsync(CancellationToken);
        downloadTask.DownloadStatus = DownloadStatus.DownloadFinished;
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.SetupCommand(It.IsAny<MoveDownloadFileFromFileTaskCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<CleanUpDownloadTaskFoldersCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);

        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue())
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.Is<DownloadStatus>(s => s == DownloadStatus.Completed),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Simulate the command handler setting MoveFinished status in the DB
        await dbContext.SetDownloadStatus(downloadTask.ToKey(), DownloadStatus.MoveFinished);

        var context = SetupJobContext(downloadTask.ToKey());

        // Act
        await Sut.Execute(context);

        // Assert
        var after = await IDbContext.GetDownloadTaskFileAsync(downloadTask.ToKey(), CancellationToken);
        after.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldNotSetStatusToCompleted_WhenMoveCommandFails()
    {
        // Arrange
        await SetupDatabase(
            11002,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.AsTracking().FirstAsync(CancellationToken);
        downloadTask.DownloadStatus = DownloadStatus.DownloadFinished;
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.SetupCommand(It.IsAny<MoveDownloadFileFromFileTaskCommand>)
            .ReturnsAsync(Result.Fail("Move failed"))
            .Verifiable(Times.Once);

        // These should NOT be called when the move command fails
        Mock.SetupCommand(It.IsAny<CleanUpDownloadTaskFoldersCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue())
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        var context = SetupJobContext(downloadTask.ToKey());

        // Act
        await Sut.Execute(context);

        // Assert: status stays at DownloadFinished (the command handler sets MoveError,
        // but since the command is mocked to fail without touching the DB, status is unchanged)
        var after = await IDbContext.GetDownloadTaskFileAsync(downloadTask.ToKey(), CancellationToken);
        after.ShouldNotBeNull();
        after.DownloadStatus.ShouldNotBe(DownloadStatus.Completed);
    }

    [Fact]
    public async Task ShouldReturnHundredPercent_WhenCompletedAndDataReceivedIsZero()
    {
        // Regression: after MoveFinished -> Completed the percentage must stay at 100 and never
        // fall back to DataReceived / DataTotal, which can be 0 for externally-managed downloads.
        // Arrange
        await SetupDatabase(
            11003,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.AsTracking().FirstAsync(CancellationToken);

        // Simulate a task whose download bytes were not tracked (e.g. external client) but whose
        // file transfer has fully completed.
        downloadTask.DataReceived = 0;
        downloadTask.DataTotal = 100_000_000;
        downloadTask.FileDataTransferred = downloadTask.DataTotal;
        downloadTask.CurrentFileTransferBytesOffset = downloadTask.DataTotal;
        downloadTask.DownloadStatus = DownloadStatus.MoveFinished;
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.SetupCommand(It.IsAny<MoveDownloadFileFromFileTaskCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<CleanUpDownloadTaskFoldersCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);

        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue())
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.Is<DownloadStatus>(s => s == DownloadStatus.Completed),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        var context = SetupJobContext(downloadTask.ToKey());

        // Act
        await Sut.Execute(context);

        // Assert
        var after = await IDbContext.GetDownloadTaskFileAsync(downloadTask.ToKey(), CancellationToken);
        after.ShouldNotBeNull();
        after.DataReceived.ShouldBe(0L); // confirm download bytes remain at 0
        after.FileDataTransferred.ShouldBe(after.DataTotal); // confirm transfer bytes still full
        after.Percentage.ShouldBe(100m); // must be 100, never reset to 0
    }
}
