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

        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);

        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue())
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
        after.DownloadStatus.ShouldBe(DownloadStatus.Completed);
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

        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue())
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

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
    public async Task ShouldDeleteDownloadWorkerTasks_WhenMoveSucceeds()
    {
        // Arrange
        await SetupDatabase(
            11003,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 4;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext
            .DownloadTaskMovieFile.AsTracking()
            .Include(x => x.DownloadWorkerTasks)
            .FirstAsync(CancellationToken);
        downloadTask.DownloadStatus = DownloadStatus.DownloadFinished;
        await dbContext.SaveChangesAsync(CancellationToken);

        var workerTaskCount = await IDbContext.DownloadWorkerTasks.CountAsync(
            x => x.DownloadTaskId == downloadTask.Id,
            CancellationToken
        );
        workerTaskCount.ShouldBe(4);

        Mock.SetupCommand(It.IsAny<MoveDownloadFileFromFileTaskCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<CleanUpDownloadTaskFoldersCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);

        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue())
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        await dbContext.SetDownloadStatus(downloadTask.ToKey(), DownloadStatus.MoveFinished);

        var context = SetupJobContext(downloadTask.ToKey());

        // Act
        await Sut.Execute(context);

        // Assert
        var remainingWorkerTasks = await IDbContext.DownloadWorkerTasks.CountAsync(
            x => x.DownloadTaskId == downloadTask.Id,
            CancellationToken
        );
        remainingWorkerTasks.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldNotThrow_WhenDownloadTaskKeyIsNull()
    {
        // Arrange
        IDictionary<string, object> dict = new Dictionary<string, object>
        {
            { MoveDownloadFileJob.DownloadTaskIdParameter, "null" },
        };
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail.JobDataMap).Returns(new JobDataMap(dict));
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.CancellationToken).Returns(CancellationToken);

        Mock.SetupCommand(It.IsAny<MoveDownloadFileFromFileTaskCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        // Act — should not throw (Quartz jobs must swallow exceptions)
        var act = async () => await Sut.Execute(Mock.Create<IJobExecutionContext>());

        // Assert
        await act.ShouldNotThrowAsync();
    }
}
