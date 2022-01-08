using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Logging;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Data.Common;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.FluentResult;
using PlexRipper.WebAPI.SignalR.Common;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace WebAPI.IntegrationTests.DownloadController
{
    [Collection("Sequential")]
    public class DownloadController_DownloadMedia_IntegrationTests : BaseIntegrationTests
    {
        public DownloadController_DownloadMedia_IntegrationTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ShouldDownloadAllTvShowEpisodes_WhenValidEpisodesAreAdded()
        {
            // Arrange
            var timer = new Stopwatch();
            timer.Start();
            var config = new UnitTestDataConfig
            {
                Seed = 4564,
                TvShowCount = 1,
                TvShowSeasonCount = 1,
                TvShowEpisodeCount = 3,
                DownloadSpeedLimit = 5000,
                MockServerConfig = new PlexMockServerConfig
                {
                    DownloadFileSizeInMb = 50,
                },
            };

            await CreateContainer(config);
            await Container.SetDownloadSpeedLimit(config);

            var downloadStreams = new List<Stream>();
            Container.TestStreamTracker.CreatedDownloadStreams.Subscribe(stream => downloadStreams.Add(stream));

            var plexTvShow = await Container.PlexRipperDbContext.PlexTvShows.IncludeEpisodes().FirstOrDefaultAsync(x => x.Id == 1);
            plexTvShow.ShouldNotBeNull();

            var request = new List<DownloadMediaDTO>
            {
                new()
                {
                    MediaIds = new List<int>
                    {
                        1,
                    },
                    Type = PlexMediaType.TvShow,
                },
            };
            var finishedDownloadTaskIds = new List<int>();
            var finishedFileMergeTasksIds = new List<int>();
            Container.GetDownloadTracker.DownloadTaskFinished.Subscribe(task => finishedDownloadTaskIds.Add(task.Id));
            Container.FileMerger.FileMergeCompletedObservable.Subscribe(progress => finishedFileMergeTasksIds.Add(progress.Id));

            // Act
            var response = await Container.PostAsync(ApiRoutes.Download.PostDownloadMedia, request);
            var result = await response.Deserialize<ResultDTO<List<ServerDownloadProgressDTO>>>();

            // ** Continue until after the application is idle
            while (finishedDownloadTaskIds.Count < config.TvShowEpisodeCount || finishedFileMergeTasksIds.Count < config.TvShowEpisodeCount)
            {
                await Task.Delay(3000);
                Log.Debug($"Number of {nameof(finishedDownloadTaskIds)} is {finishedDownloadTaskIds.Count}" +
                          $" and {nameof(finishedFileMergeTasksIds)} is {finishedFileMergeTasksIds.Count}, continue waiting");
            }

            await Task.Delay(5000);

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            result.IsSuccess.ShouldBeTrue();

            // ** 4 streams per download client should be created
            var downloadTasks = await Container.PlexRipperDbContext.DownloadTasks.ToListAsync();
            downloadTasks.Count.ShouldBe(config.TvShowEpisodeCount + 2);
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.DownloadStatus.ShouldBe(DownloadStatus.Completed);
            }

            downloadStreams.Count.ShouldBe(config.TvShowEpisodeCount * 4);

            timer.Stop();
            Log.Information($"Test took: {timer.Elapsed}");
        }
    }
}