using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Logging;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.Data;
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
    }
}