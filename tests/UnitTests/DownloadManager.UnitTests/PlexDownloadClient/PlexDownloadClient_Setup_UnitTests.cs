using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using FluentResults;
using Logging;
using Moq;
using PlexRipper.Application;
using PlexRipper.Application.Common;
using PlexRipper.Application.DownloadWorkerTasks;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Download;
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
            using var mock = AutoMock.GetStrict();
            mock.SetupMediator(It.IsAny<AddDownloadWorkerTasksCommand>).ReturnsAsync(Result.Ok());
            mock.SetupMediator(It.IsAny<GetAllDownloadWorkerTasksByDownloadTaskIdQuery>)
                .ReturnsAsync(Result.Ok(FakeData.GetDownloadWorkerTask().Generate(4)));

            mock.Mock<IUserSettings>().SetupGet(x => x.DownloadSegments).Returns(4);

            var downloadTask = FakeData.GetMovieDownloadTask().Generate();
            downloadTask.DownloadWorkerTasks = new List<DownloadWorkerTask>();

            // Act
            var result = await mock.Create<PlexDownloadClient>().Setup(downloadTask);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.DownloadTask.DownloadWorkerTasks.Count.ShouldBe(4);
        }
    }
}