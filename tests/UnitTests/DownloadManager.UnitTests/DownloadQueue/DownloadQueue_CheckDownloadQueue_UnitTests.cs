using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using FluentResults;
using Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Data.Common;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class DownloadQueue_CheckDownloadQueue_UnitTests
    {
        public DownloadQueue_CheckDownloadQueue_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldHaveNoUpdates_WhenGivenAnEmptyList()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.SetupMediator(It.IsAny<GetAllDownloadTasksInPlexServersQuery>)
                .ReturnsAsync(Result.Ok(new List<PlexServer>()));
            var _sut = mock.Create<DownloadQueue>();

            List<DownloadTask> updates = new();
            List<DownloadTask> startCommands = new();

            // Act
            _sut.UpdateDownloadTasks.Subscribe(update => updates = update);
            _sut.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            _sut.Setup();
            _sut.CheckDownloadQueue();

            // Assert
            updates.Any().ShouldBeFalse();
            startCommands.Any().ShouldBeFalse();
        }

        [Fact]
        public async Task ShouldHaveNoStartCommands_WhenATaskIsAlreadyDownloading()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            var config = new UnitTestDataConfig
            {
                MovieDownloadTasksCount = 5,
            };
            await using var context = MockDatabase.GetMemoryDbContext().Setup(config);
            var downloadTasks = await context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToListAsync();
            mock.SetupMediator(It.IsAny<GetDownloadTasksByPlexServerIdQuery>)
                .ReturnsAsync((GetDownloadTasksByPlexServerIdQuery query, CancellationToken _) =>
                    Result.Ok(downloadTasks.Where(x => x.PlexServerId == query.PlexServerId).ToList()));

            List<DownloadTask> startCommands = new();
            var plexServers = await context.PlexServers
                .AsTracking()
                .Include(x => x.PlexLibraries)
                .ThenInclude(x => x.DownloadTasks)
                .ToListAsync();

            var startedDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0];
            startedDownloadTask.DownloadStatus = DownloadStatus.Downloading;
            startedDownloadTask.Children.SetToDownloading();
            await context.SaveChangesAsync();

            // Act
            _sut.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            await _sut.CheckDownloadQueueServer(1);

            // Assert
            startCommands.Any().ShouldBeFalse();
        }

        [Fact]
        public async Task ShouldHaveNoDownloadTasksInitialized_WhenGivenDownloadTasksWithInitialized()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            int updateIndex = 1;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new UnitTestDataConfig
            {
                MovieDownloadTasksCount = 10,
            };
            await using var context = MockDatabase.GetMemoryDbContext().Setup(config);
            var downloadTasks = await context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToListAsync();
            mock.SetupMediator(It.IsAny<GetDownloadTasksByPlexServerIdQuery>)
                .ReturnsAsync((GetDownloadTasksByPlexServerIdQuery query, CancellationToken _) =>
                    Result.Ok(downloadTasks.Where(x => x.PlexServerId == query.PlexServerId).ToList()));

            // Act
            _sut.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            await _sut.CheckDownloadQueueServer(1);

            // Assert
            var downloadTasksUpdate = updates[updateIndex];
            downloadTasksUpdate.Any().ShouldBeTrue();
            downloadTasksUpdate.All(x => x.DownloadStatus is not DownloadStatus.Initialized).ShouldBeTrue();
        }

        [Fact]
        public void ShouldHaveOneDownloadTaskStarted_WhenGivenMovieDownloadTasks()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            List<DownloadTask> startCommands = new();
            var config = new UnitTestDataConfig
            {
                Seed = 5000,
                IncludeLibraries = true,
                PlexLibraryCount = 1,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Act
            _sut.StartDownloadTask.Subscribe(command => startCommands.Add(command));

            // _sut.CheckDownloadQueue(plexServers);

            // Assert
            var startedDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0].Children[0];
            startCommands.Count.ShouldBe(1);
            startCommands[0].Id.ShouldBe(startedDownloadTask.Id);
        }

        [Fact]
        public void ShouldHaveOneDownloadTaskDownloadingStatus_WhenGivenMovieDownloadTasks()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            int updateIndex = 1;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new UnitTestDataConfig
            {
                Seed = 25,
                IncludeLibraries = true,
                PlexLibraryCount = 1,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Act
            _sut.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));

            // _sut.CheckDownloadQueue(plexServers);

            // Assert

            var downloadTasks = updates[updateIndex];
            downloadTasks.Any().ShouldBeTrue();
            downloadTasks[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasks[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);

            downloadTasks[1].DownloadStatus.ShouldBe(DownloadStatus.Queued);
            downloadTasks[1].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Queued);
        }

        [Fact]
        public void ShouldHaveOneDownloadTaskDownloadingStatus_WhenGivenTvShowDownloadTasks()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            int updateIndex = 1;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new UnitTestDataConfig
            {
                Seed = 67,
                IncludeLibraries = true,
                PlexLibraryCount = 1,
                LibraryType = PlexMediaType.TvShow,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Act
            _sut.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));

            // _sut.CheckDownloadQueue(plexServers);

            // Assert

            var downloadTasks = updates[updateIndex];
            downloadTasks.Any().ShouldBeTrue();
            downloadTasks[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasks[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);

            downloadTasks[1].DownloadStatus.ShouldBe(DownloadStatus.Queued);
            downloadTasks[1].Children.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Queued);
        }

        [Fact]
        public async Task ShouldHaveNextQueuedDownloadTask_WhenGivenAMovieDownloadTasksWithCompleted()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 67,
                MovieDownloadTasksCount = 5,
            };
            await using var context = MockDatabase.GetMemoryDbContext().Setup(config);
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            var downloadTasks = await context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToListAsync();
            mock.SetupMediator(It.IsAny<GetDownloadTasksByPlexServerIdQuery>)
                .ReturnsAsync((GetDownloadTasksByPlexServerIdQuery query, CancellationToken _) =>
                    Result.Ok(downloadTasks.Where(x => x.PlexServerId == query.PlexServerId).ToList()));

            int updateIndex = 0;
            Dictionary<int, List<DownloadTask>> updates = new();

            // Set first task to Completed
            var movieDownloadTask = context.DownloadTasks.Include(x => x.Children).AsTracking().First();
            movieDownloadTask.DownloadStatus = DownloadStatus.Completed;
            movieDownloadTask.Children.ForEach(x => x.DownloadStatus = DownloadStatus.Completed);
            await context.SaveChangesAsync();

            // Act
            _sut.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            await _sut.CheckDownloadQueueServer(downloadTasks.First().PlexServerId);

            // Assert
            var downloadTasksResult = updates[updateIndex];
            downloadTasksResult.Any().ShouldBeTrue();
            downloadTasksResult[1].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasksResult[1].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
        }

        [Fact]
        public async Task ShouldHaveNextQueuedDownloadTask_WhenGivenATvShowsDownloadTasksWithCompleted()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 263,
                TvShowDownloadTasksCount = 2,
            };
            await using var context = MockDatabase.GetMemoryDbContext().Setup(config);
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();

            var downloadTasks = await context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToListAsync();
            mock.SetupMediator(It.IsAny<GetDownloadTasksByPlexServerIdQuery>)
                .ReturnsAsync((GetDownloadTasksByPlexServerIdQuery query, CancellationToken _) =>
                    Result.Ok(downloadTasks.Where(x => x.PlexServerId == query.PlexServerId).ToList()));

            int updateIndex = 0;
            Dictionary<int, List<DownloadTask>> updates = new();
            var plexServers = await context.PlexServers
                .AsTracking()
                .Include(x => x.PlexLibraries)
                .ThenInclude(x => x.DownloadTasks)
                .ToListAsync();

            // Set first task to Completed
            var tvShowDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0];
            tvShowDownloadTask.DownloadStatus = DownloadStatus.Completed;
            tvShowDownloadTask.Children = tvShowDownloadTask.Children.SetToCompleted();
            await context.SaveChangesAsync();

            // Act
            _sut.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            await _sut.CheckDownloadQueueServer(1);

            // Assert
            var downloadTasksResult = updates[updateIndex];
            downloadTasksResult.Any().ShouldBeTrue();
            downloadTasksResult[1].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasksResult[1].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasksResult[1].Children[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasksResult[1].Children[0].Children[1].DownloadStatus.ShouldBe(DownloadStatus.Queued);
            downloadTasksResult[1].Children[0].Children[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
        }
    }
}