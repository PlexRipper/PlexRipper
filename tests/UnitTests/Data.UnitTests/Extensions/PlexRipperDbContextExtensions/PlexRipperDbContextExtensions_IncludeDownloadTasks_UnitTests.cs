using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logging;
using PlexRipper.BaseTests;
using PlexRipper.Data.Common;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Data.UnitTests.Extensions
{
    public class PlexRipperDbContextExtensions_IncludeDownloadTasks_UnitTests
    {
        public PlexRipperDbContextExtensions_IncludeDownloadTasks_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldHaveAllMovieDownloadTaskChildrenIncluded_WhenDbContainsNestedDownloadTasks()
        {
            // Arrange
            await using var context = MockDatabase.GetMemoryDbContext();
            var fakeDownloadTasks = FakeData.GetMovieDownloadTask().Generate(5);
            context.DownloadTasks.AddRange(fakeDownloadTasks);
            await context.SaveChangesAsync();

            // Act
            var downloadTasks = context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToList();

            // Assert
            downloadTasks.Count.ShouldBe(5);
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.Children.Count.ShouldBe(1);
                downloadTask.Children[0].ParentId.ShouldBe(downloadTask.Id);
            }
        }

        [Fact]
        public async Task ShouldHaveAllTvShowDownloadTaskChildrenIncluded_WhenDbContainsNestedDownloadTasks()
        {
            // Arrange
            await using var context = MockDatabase.GetMemoryDbContext();
            var fakeDownloadTasks = FakeData.GetTvShowDownloadTask().Generate(5);
            context.DownloadTasks.AddRange(fakeDownloadTasks);
            await context.SaveChangesAsync();

            // Act
            var downloadTasksDb = context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToList();

            // Assert
            downloadTasksDb.Count.ShouldBe(5);
            downloadTasksDb.Flatten(x => x.Children).Count().ShouldBe(fakeDownloadTasks.Flatten(x => x.Children).Count());
        }

        [Fact]
        public async Task ShouldHaveAllNestedRelationshipsIncluded_WhenGivenTvShowDownloadTasks()
        {
            // Arrange
            await using var context = MockDatabase.GetMemoryDbContext();
            var fakeDownloadTasks = FakeData.GetTvShowDownloadTask().Generate(5);
            context.DownloadTasks.AddRange(fakeDownloadTasks);
            await context.SaveChangesAsync();

            // Act
            var downloadTasksDb = context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToList();

            // Assert
            downloadTasksDb.Count.ShouldBe(5);
            CheckChildren(downloadTasksDb);

            void CheckChildren(List<DownloadTask> downloadTasks)
            {
                if (downloadTasks.Any())
                {
                    foreach (var downloadTask in downloadTasks)
                    {
                        downloadTask.PlexServer.ShouldNotBeNull();
                        downloadTask.PlexLibrary.ShouldNotBeNull();
                        downloadTask.DownloadFolder.ShouldNotBeNull();
                        downloadTask.DestinationFolder.ShouldNotBeNull();
                        if (downloadTask.Children.Any())
                        {
                            CheckChildren(downloadTask.Children);
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task ShouldHavePlexServerAndLibraryIncluded_WhenDownloadTasksAreValid()
        {
            // Arrange
            await using var context = MockDatabase.GetMemoryDbContext();
            var fakeDownloadTasks = FakeData.GetMovieDownloadTask().Generate(5);
            context.DownloadTasks.AddRange(fakeDownloadTasks);
            await context.SaveChangesAsync();

            // Act
            var downloadTasks = context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToList();

            // Assert
            downloadTasks.Count.ShouldBe(5);
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.Parent.ShouldBeNull();
                downloadTask.PlexServer.ShouldNotBeNull();
                downloadTask.PlexLibrary.ShouldNotBeNull();
                downloadTask.Children.Count.ShouldBe(1);
                downloadTask.Children[0].ParentId.ShouldBe(downloadTask.Id);
                downloadTask.Children[0].PlexServer.ShouldNotBeNull();
                downloadTask.Children[0].PlexLibrary.ShouldNotBeNull();
            }
        }

        [Fact]
        public async Task ShouldHaveDownloadFolderAndDestinationFolderIncluded_WhenDownloadTasksAreValid()
        {
            // Arrange
            await using var context = MockDatabase.GetMemoryDbContext();
            var fakeDownloadTasks = FakeData.GetMovieDownloadTask().Generate(5);
            context.DownloadTasks.AddRange(fakeDownloadTasks);
            await context.SaveChangesAsync();

            // Act
            var downloadTasks = context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToList();

            // Assert
            downloadTasks.Count.ShouldBe(5);
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.Parent.ShouldBeNull();
                downloadTask.DownloadFolder.ShouldNotBeNull();
                downloadTask.DestinationFolder.ShouldNotBeNull();
                downloadTask.Children.Count.ShouldBe(1);
                downloadTask.Children[0].ParentId.ShouldBe(downloadTask.Id);
                downloadTask.Children[0].DownloadFolder.ShouldNotBeNull();
                downloadTask.Children[0].DestinationFolder.ShouldNotBeNull();
            }
        }

        [Fact]
        public async Task ShouldHaveDownloadTaskParentIncluded_WhenDownloadTasksAreValid()
        {
            // Arrange
            await using var context = MockDatabase.GetMemoryDbContext();
            var fakeDownloadTasks = FakeData.GetMovieDownloadTask().Generate(5);
            context.DownloadTasks.AddRange(fakeDownloadTasks);
            await context.SaveChangesAsync();

            // Act
            var downloadTasks = context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToList();

            // Assert
            downloadTasks.Count.ShouldBe(5);
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.Parent.ShouldBeNull();
                downloadTask.Children.Count.ShouldBe(1);
                downloadTask.Children[0].ParentId.ShouldBe(downloadTask.Id);
                downloadTask.Children[0].Parent.ShouldNotBeNull();
            }
        }
    }
}