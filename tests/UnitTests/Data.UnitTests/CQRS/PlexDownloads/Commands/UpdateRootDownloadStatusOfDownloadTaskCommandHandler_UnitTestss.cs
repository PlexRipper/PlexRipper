using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Data;
using PlexRipper.Data.Common;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Data.UnitTests.Commands
{
    public class UpdateRootDownloadStatusOfDownloadTaskCommandHandler_UnitTests
    {
        public UpdateRootDownloadStatusOfDownloadTaskCommandHandler_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldAllBeQueuedDownloadTasks_WhenAllChildrenAreQueuedStatus()
        {
            // Arrange
            await using var context = await MockDatabase.GetMemoryDbContext(disableForeignKeyCheck: true).Setup(config =>
            {
                config.Seed = 9679;
                config.TvShowDownloadTasksCount = 1;
            });
            var downloadTasks = await context.DownloadTasks.IncludeDownloadTasks().IncludeByRoot().ToListAsync();
            var request = new UpdateRootDownloadStatusOfDownloadTaskCommand(downloadTasks.First().Id);
            var handler = new UpdateRootDownloadStatusOfDownloadTaskCommandHandler(context);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var flattenDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
            var downloadTasksDb = await context.DownloadTasks.ToListAsync();
            downloadTasksDb.Count.ShouldBe(flattenDownloadTasks.Count);
            downloadTasksDb.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Queued);
        }

        [Fact]
        public async Task ShouldAllBeCompletedDownloadTasks_WhenOnlyAllChildrenAreCompletedStatus()
        {
            // Arrange
            await using var context = await MockDatabase.GetMemoryDbContext(disableForeignKeyCheck: true).Setup(config =>
            {
                config.Seed = 9999;
                config.TvShowDownloadTasksCount = 1;
            });
            var downloadTasks = await context.DownloadTasks.IncludeDownloadTasks().IncludeByRoot().ToListAsync();
            foreach (var seasonDownloadTask in downloadTasks[0].Children)
            {
                foreach (var episodeDownloadTask in seasonDownloadTask.Children)
                {
                    episodeDownloadTask.Children = episodeDownloadTask.Children.SetToCompleted();
                }
            }

            var request = new UpdateRootDownloadStatusOfDownloadTaskCommand(downloadTasks.First().Id);
            var handler = new UpdateRootDownloadStatusOfDownloadTaskCommandHandler(context);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var flattenDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
            var downloadTasksDb = await context.DownloadTasks.ToListAsync();
            downloadTasksDb.Count.ShouldBe(flattenDownloadTasks.Count);
            downloadTasksDb.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
        }

        [Fact]
        public async Task ShouldBeInErrorStatus_WhenOneChildHasErrorStatus()
        {
            // Arrange
            await using PlexRipperDbContext context = await MockDatabase.GetMemoryDbContext(disableForeignKeyCheck: true).Setup(config =>
            {
                config.Seed = 9999;
                config.TvShowDownloadTasksCount = 1;
            });
            var downloadTasks = await context.DownloadTasks.IncludeDownloadTasks().IncludeByRoot().ToListAsync();
            downloadTasks[0].Children[0].Children[0].Children[0].DownloadStatus = DownloadStatus.Error;

            var request = new UpdateRootDownloadStatusOfDownloadTaskCommand(downloadTasks.First().Id);
            var handler = new UpdateRootDownloadStatusOfDownloadTaskCommandHandler(context);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var flattenDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
            var downloadTasksDb = await context.DownloadTasks.ToListAsync();
            downloadTasksDb.Count.ShouldBe(flattenDownloadTasks.Count);

            var downloadTaskDb = context.DownloadTasks.IncludeDownloadTasks().FirstOrDefault(x => x.Id == 1);
            downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Error);
            downloadTaskDb.Children[0].DownloadStatus.ShouldBe(DownloadStatus.Error);
            downloadTaskDb.Children[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Error);
            downloadTaskDb.Children[0].Children[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Error);
            downloadTaskDb.Children[1].DownloadStatus.ShouldBe(DownloadStatus.Queued);
            downloadTaskDb.Children[1].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Queued);
            downloadTaskDb.Children[1].Children[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Queued);
        }
    }
}