using EFCore.BulkExtensions;
using Logging;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.Data;
using PlexRipper.Data.Common;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Data.UnitTests.Commands
{
    public class CreateDownloadTasksCommandHandler_UnitTests
    {
        public CreateDownloadTasksCommandHandler_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldCreateAllDownloadTasks_WhenAllAreNew()
        {
            // Arrange
            var downloadTasks = FakeData.GetTvShowDownloadTask().Generate(1);
            await using var context = MockDatabase.GetMemoryDbContext(disableForeignKeyCheck: true);
            var request = new CreateDownloadTasksCommand(downloadTasks);
            var handler = new CreateDownloadTasksCommandHandler(context);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var flattenDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
            context.DownloadTasks.Count().ShouldBe(flattenDownloadTasks.Count);
        }

        [Fact]
        public async Task ShouldCreateOnlyChildDownloadTasks_WhenParentAlreadyExists()
        {
            // Arrange
            var downloadTasks = FakeData.GetTvShowDownloadTask().Generate(1);
            await using var context = MockDatabase.GetMemoryDbContext(disableForeignKeyCheck: true);

            await context.BulkInsertAsync(new List<DownloadTask> { downloadTasks.First() });
            downloadTasks[0].Id = 1;
            var request = new CreateDownloadTasksCommand(downloadTasks);
            var handler = new CreateDownloadTasksCommandHandler(context);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var flattenDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
            context.DownloadTasks.Count().ShouldBe(flattenDownloadTasks.Count);
        }

        [Fact]
        public async Task ShouldAllHaveARootDownloadTaskId_WhenDownloadTasksAreChildren()
        {
            // Arrange
            var downloadTasksTest = FakeData.GetTvShowDownloadTask().Generate(1);
            await using var context = MockDatabase.GetMemoryDbContext(disableForeignKeyCheck: true);
            var request = new CreateDownloadTasksCommand(downloadTasksTest);
            var handler = new CreateDownloadTasksCommandHandler(context);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var downloadTasksDb = await context.DownloadTasks.IncludeDownloadTasks().IncludeByRoot().ToListAsync();

            void HasRootDownloadTaskId(List<DownloadTask> downloadTasks)
            {
                foreach (var downloadTask in downloadTasks)
                {
                    downloadTask.RootDownloadTaskId.ShouldBe(1);
                    if (downloadTask.Children.Any())
                    {
                        HasRootDownloadTaskId(downloadTask.Children);
                    }
                }
            }

            HasRootDownloadTaskId(downloadTasksDb.SelectMany(x => x.Children).ToList());
        }
    }
}