using System.Collections.Generic;
using Autofac.Extras.Moq;
using Logging;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class DownloadQueue_GetNextDownloadTask_UnitTests
    {
        public DownloadQueue_GetNextDownloadTask_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        private List<DownloadTask> TestDownloadTasks(int count)
        {
            var downloadTasks = new List<DownloadTask>();
            int index = 1;
            for (var i = 0; i < count; i++)
            {
                var downloadTask_i = new DownloadTask
                {
                    Id = index++,
                    DownloadStatus = DownloadStatus.Queued,
                    Children = new List<DownloadTask>(),
                };
                for (var j = 0; j < count; j++)
                {
                    var downloadTask_j = new DownloadTask
                    {
                        Id = index++,
                        DownloadStatus = DownloadStatus.Queued,
                        Children = new List<DownloadTask>(),
                    };
                    for (var k = 0; k < count; k++)
                    {
                        var downloadTask_k = new DownloadTask
                        {
                            Id = index++,
                            DownloadStatus = DownloadStatus.Queued,
                            Children = new List<DownloadTask>(),
                        };
                        for (var l = 0; l < count; l++)
                        {
                            var downloadTask_l = new DownloadTask
                            {
                                Id = index++,
                                DownloadStatus = DownloadStatus.Queued,
                                Children = new List<DownloadTask>(),
                            };
                            downloadTask_k.Children.Add(downloadTask_l);
                        }

                        downloadTask_j.Children.Add(downloadTask_k);
                    }

                    downloadTask_i.Children.Add(downloadTask_j);
                }

                downloadTasks.Add(downloadTask_i);
            }

            return downloadTasks;
        }

        [Fact]
        public void ShouldHaveNextDownloadTask_WhenAllAreQueued()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            var downloadTasks = TestDownloadTasks(2);

            // Act
            var nextDownloadTask = DownloadQueue.GetNextDownloadTask(ref downloadTasks);

            // Assert
            nextDownloadTask.IsSuccess.ShouldBeTrue();
            nextDownloadTask.Value.Id.ShouldBe(4);
        }

        [Fact]
        public void ShouldHaveNextDownloadTask_WhenADownloadTaskHasBeenCompleted()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            var downloadTasks = TestDownloadTasks(3);
            downloadTasks[0].DownloadStatus = DownloadStatus.Completed;
            downloadTasks[0].Children = downloadTasks[0].Children.SetToCompleted();

            // Act
            var nextDownloadTask = DownloadQueue.GetNextDownloadTask(ref downloadTasks);

            // Assert
            nextDownloadTask.IsSuccess.ShouldBeTrue();
            nextDownloadTask.Value.Id.ShouldBe(44);
        }

        [Fact]
        public void ShouldHaveNextQueuedDownloadTaskInDownloadingTask_WhenAParentDownloadTaskIsAlreadyDownloading()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            var downloadTasks = TestDownloadTasks(3);
            downloadTasks[0].DownloadStatus = DownloadStatus.Downloading;

            // Act
            var nextDownloadTask = DownloadQueue.GetNextDownloadTask(ref downloadTasks);

            // Assert
            nextDownloadTask.IsSuccess.ShouldBeTrue();
            nextDownloadTask.Value.Id.ShouldBe(4);
        }

        [Fact]
        public void ShouldHaveNoDownloadTask_WhenADownloadTaskIsAlreadyDownloading()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            var downloadTasks = TestDownloadTasks(2);
            downloadTasks[0].DownloadStatus = DownloadStatus.Downloading;
            downloadTasks[0].Children = downloadTasks[0].Children.SetToDownloading();

            // Act
            var nextDownloadTask = DownloadQueue.GetNextDownloadTask(ref downloadTasks);

            // Assert
            nextDownloadTask.IsSuccess.ShouldBeFalse();
        }
    }
}