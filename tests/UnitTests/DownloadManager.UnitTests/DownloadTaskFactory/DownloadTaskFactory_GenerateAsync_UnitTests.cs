using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using Moq;
using PlexRipper.Application;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Config;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class DownloadTaskFactory_GenerateAsync_UnitTests
    {
        private readonly Mock<PlexRipper.DownloadManager.DownloadTaskFactory> _sut;

        private readonly Mock<IMediator> _iMediator = new();

        private readonly Mock<IPlexAuthenticationService> _plexAuthenticationService = new();

        private readonly Mock<INotificationsService> _notificationsService = new();

        private readonly Mock<IFolderPathService> _folderPathService = new();

        private readonly Mock<IUserSettings> _userSettings = new();

        public DownloadTaskFactory_GenerateAsync_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);

            _sut = new Mock<PlexRipper.DownloadManager.DownloadTaskFactory>(
                MockBehavior.Strict,
                _iMediator.Object,
                MapperSetup.CreateMapper(),
                _plexAuthenticationService.Object,
                _notificationsService.Object,
                _folderPathService.Object,
                _userSettings.Object);
        }

        [Fact]
        public async Task ShouldHaveFailedResult_WhenFolderPathsAreInvalid()
        {
            // Arrange
            _iMediator.Setup(x => x.Send(It.IsAny<GetPlexTvShowTreeByMediaIdsQuery>(), CancellationToken.None)).ReturnsAsync(Result.Fail(""));
            _folderPathService.Setup(x => x.CheckIfFolderPathsAreValid(PlexMediaType.None)).ReturnsAsync(Result.Fail(""));
            var tvShowIds = new List<int>();
            var tvShowSeasonIds = new List<int>();
            var tvShowEpisodeIds = new List<int>();

            var downloadMedias = new List<DownloadMediaDTO>
            {
                new()
                {
                    Type = PlexMediaType.TvShow,
                    MediaIds = tvShowIds,
                },
                new()
                {
                    Type = PlexMediaType.Season,
                    MediaIds = tvShowSeasonIds,
                },
                new()
                {
                    Type = PlexMediaType.Episode,
                    MediaIds = tvShowEpisodeIds,
                },
            };

            // Act
            var result = await _sut.Object.GenerateAsync(downloadMedias);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }
    }
}