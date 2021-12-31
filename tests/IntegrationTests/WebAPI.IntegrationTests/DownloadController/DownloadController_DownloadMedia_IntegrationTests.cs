﻿using System;
using System.Collections.Generic;
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
    public class DownloadController_DownloadMedia_IntegrationTests
    {
        public DownloadController_DownloadMedia_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldHaveAllDownloadTasksNested_WhenTasksAreAvailable()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 4564,
                TvShowCount = 1,
                TvShowSeasonCount = 1,
                TvShowEpisodeCount = 5,
                DownloadSpeedLimit = 5000,
                MockServerConfig = new PlexMockServerConfig
                {
                    DownloadFileSize = 50,
                },
            };

            var container = await BaseContainer.Create(config);
            await container.SetDownloadSpeedLimit(config);

            var downloadStreams = new List<Stream>();
            container.TestStreamTracker.CreatedDownloadStreams.Subscribe(stream =>
                downloadStreams.Add(stream)
            );

            var plexTvShow = await container.PlexRipperDbContext.PlexTvShows.IncludeEpisodes().FirstOrDefaultAsync(x => x.Id == 1);
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

            // Act
            var response = await container.PostAsync(ApiRoutes.Download.PostDownloadMedia, request);
            var result = await response.Deserialize<ResultDTO<List<ServerDownloadProgressDTO>>>();

            // ** Continue after the application has idle
            await container.TestApplicationTracker.WaitUntilApplicationIsIdle(logStatus: true);
            await Task.Delay(10000);

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            result.IsSuccess.ShouldBeTrue();

            // ** 4 streams per download client should be created
            var downloadTasks = await container.PlexRipperDbContext.DownloadTasks.ToListAsync();
            downloadTasks.Count.ShouldBe(7);
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.DownloadStatus.ShouldBe(DownloadStatus.Completed);
            }

            downloadStreams.Count.ShouldBe(config.TvShowEpisodeCount * 4);
        }
    }
}