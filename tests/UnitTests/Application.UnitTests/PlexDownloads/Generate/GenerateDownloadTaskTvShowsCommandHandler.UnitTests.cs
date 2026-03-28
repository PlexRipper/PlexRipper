using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class GenerateDownloadTaskTvShowsCommandHandlerUnitTests
    : BaseUnitTest<GenerateDownloadTaskTvShowsCommandHandler>
{
    public GenerateDownloadTaskTvShowsCommandHandlerUnitTests()
        : base() { }

    [Test]
    public async Task ShouldHaveInsertedValidDownloadTaskTvShowsInDatabase_WhenGivenValidPlexTvShows()
    {
        // Arrange
        await SetupDatabase(
            5954,
            config =>
            {
                config.TvShowCount = 5;
                config.TvShowSeasonCount = 3;
                config.TvShowEpisodeCount = 3;
            }
        );
        var plexTvShows = await IDbContext.PlexTvShows.IncludeAll().ToListAsync(CancellationToken);

        var tvShows = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.TvShow,
                MediaIds = plexTvShows.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());

        // Act
        var command = new GenerateDownloadTaskTvShowsCommand(tvShows);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var downloadTaskTvShows = await IDbContext.GetAllDownloadTasksByServerAsync(
            cancellationToken: CancellationToken
        );

        downloadTaskTvShows.Count.ShouldBe(5);

        foreach (var downloadTaskTvShow in downloadTaskTvShows)
        {
            downloadTaskTvShow.Id.ShouldNotBe(Guid.Empty);
            downloadTaskTvShow.RatingKey.ShouldBeGreaterThan(0);
            downloadTaskTvShow.Title.ShouldNotBeEmpty();
            downloadTaskTvShow.FullTitle.ShouldNotBeEmpty();
            downloadTaskTvShow.DownloadStatus.ShouldBe(DownloadStatus.Queued);

            downloadTaskTvShow.PlexServerId.ShouldBe(1);
            downloadTaskTvShow.PlexLibraryId.ShouldBe(1);

            downloadTaskTvShow.DownloadTaskType.ShouldBe(DownloadTaskType.TvShow);
            downloadTaskTvShow.MediaType.ShouldBe(PlexMediaType.TvShow);
            downloadTaskTvShow.Children.ShouldBeEmpty();
        }

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldForwardCustomDestinationFolderPath_WhenCreatingSeasonsCommand()
    {
        // Arrange
        await SetupDatabase(
            5955,
            config =>
            {
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 1;
            }
        );
        var plexTvShows = await IDbContext.PlexTvShows.IncludeAll().ToListAsync(CancellationToken);

        var tvShows = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.TvShow,
                MediaIds = plexTvShows.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        const string customPath = "/custom/destination/path";
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());

        // Act
        var request = new CreateDownloadTasksRequest(tvShows, null, customPath);
        var command = new GenerateDownloadTaskTvShowsCommand(request);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<GenerateDownloadTaskTvShowSeasonsCommand>(cmd =>
                            cmd.Request.CustomDestinationFolderPath == customPath
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }
}
