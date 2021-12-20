using System.Linq;
using Autofac.Extras.Moq;
using Logging;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class DownloadQueue_SetToCompleted_UnitTests
    {
        public DownloadQueue_SetToCompleted_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldSetMovieParentToCompleted_WhenAllChildrenAreCompleted()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();

            var movieDownloadTasks = FakeData.GetMovieDownloadTask().Generate(2);

            movieDownloadTasks[0].DownloadStatus = DownloadStatus.Queued;
            movieDownloadTasks[0].Children[0].DownloadStatus = DownloadStatus.Completed;
            movieDownloadTasks[1].DownloadStatus = DownloadStatus.Queued;
            movieDownloadTasks[1].Children[0].DownloadStatus = DownloadStatus.Queued;

            // Act
            var downloadTasksResult = _sut.SetToCompleted(movieDownloadTasks);

            // Assert
            downloadTasksResult.Any().ShouldBe(true);
            downloadTasksResult.Count.ShouldBe(2);
            movieDownloadTasks[0].DownloadStatus.ShouldBe(DownloadStatus.Completed);
            movieDownloadTasks[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Completed);
            movieDownloadTasks[1].DownloadStatus.ShouldBe(DownloadStatus.Queued);
            movieDownloadTasks[1].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Queued);
        }

        [Fact]
        public void ShouldSetTvShowParentToCompleted_WhenAllChildrenAreCompleted()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();

            var tvShowDownloadTasks = FakeData.GetTvShowDownloadTask().Generate(2);

            foreach (var seasonDownloadTask in tvShowDownloadTasks[0].Children)
            {
                foreach (var episodeDownloadTask in seasonDownloadTask.Children)
                {
                    foreach (var episodeDataDownloadTask in episodeDownloadTask.Children)
                    {
                        episodeDataDownloadTask.DownloadStatus = DownloadStatus.Completed;
                    }
                }
            }

            // Act
            var downloadTasksResult = _sut.SetToCompleted(tvShowDownloadTasks);

            // Assert
            downloadTasksResult.Any().ShouldBe(true);
            downloadTasksResult.Count.ShouldBe(2);
            downloadTasksResult[0].DownloadStatus.ShouldBe(DownloadStatus.Completed);
            foreach (var seasonDownloadTask in downloadTasksResult[0].Children)
            {
                seasonDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Completed);
                foreach (var episodeDownloadTask in seasonDownloadTask.Children)
                {
                    episodeDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Completed);
                    foreach (var episodeDataDownloadTask in episodeDownloadTask.Children)
                    {
                        episodeDataDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Completed);
                    }
                }
            }

            foreach (var seasonDownloadTask in downloadTasksResult[1].Children)
            {
                seasonDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Queued);
                foreach (var episodeDownloadTask in seasonDownloadTask.Children)
                {
                    episodeDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Queued);
                    foreach (var episodeDataDownloadTask in episodeDownloadTask.Children)
                    {
                        episodeDataDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Queued);
                    }
                }
            }
        }
    }
}