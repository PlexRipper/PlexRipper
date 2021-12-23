using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using FluentResults;
using Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.DownloadClient;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class PlexDownloadClient_Setup_UnitTests
    {
        public PlexDownloadClient_Setup_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldReturnFailedResult_WhenNullDownloadTaskIsGiven()
        {
            //Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IDownloadQueue>().SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());
            var _sut = mock.Create<PlexDownloadClient>();

            // Act
            var result = await _sut.Setup(null);

            // Assert
            result.IsFailed.ShouldBeTrue();
            result.Errors.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task ShouldReturnFailedResult_WhenDownloadTaskPlexServerIsNull()
        {
            //Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<PlexDownloadClient>();
            var downloadTask = FakeData.GetMovieDownloadTask().Generate();
            downloadTask.PlexServer = null;

            // Act
            var result = await _sut.Setup(downloadTask);

            // Assert
            result.IsFailed.ShouldBeTrue();
            result.Errors.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task ShouldGenerateDownloadWorkerTasks_WhenDownloadTaskHasNoDownloadWorkerTasks()
        {
            //Arrange
            var config = new UnitTestDataConfig()
            {
                MovieDownloadTasksCount = 1,
            };
            await using var context = MockDatabase.GetMemoryDbContext().Setup(config);

            using var mock = AutoMock.GetStrict();
            mock.SetupMediator(It.IsAny<AddDownloadWorkerTasksCommand>).ReturnsAsync(Result.Ok());
            mock.Mock<IUserSettings>().SetupGet(x => x.DownloadSegments).Returns(4);

            var downloadTask = context.DownloadTasks.Include(x => x.PlexServer).First(x => x.DownloadTaskType == DownloadTaskType.Movie);

            // Act
            var result = await mock.Create<PlexDownloadClient>().Setup(downloadTask);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.DownloadTask.DownloadWorkerTasks.Count.ShouldBe(4);
        }
    }
}