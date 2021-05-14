using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class PlexDownloadClientTests
    {
        private BaseContainer Container { get; }

        public PlexDownloadClientTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task TestDownloadClient()
        {
            // Arrange
            await Container.GetFolderPathService.UpdateFolderPathAsync(new FolderPath(FolderType.DownloadFolder, "DownloadFolder",
                @"D:\PlexDownloadFolder\") { Id = 1 });
            await Container.GetFolderPathService.UpdateFolderPathAsync(new FolderPath(FolderType.MovieFolder, "MovieFolder",
                @"D:\PlexDestinationFolder\Movies") { Id = 2 });
            await Container.GetFolderPathService.UpdateFolderPathAsync(new FolderPath(FolderType.MovieFolder, "MovieFolder",
                @"D:\PlexDestinationFolder\TvShows") { Id = 3 });

            PlexAccount testAccount = await Container.SetupTestAccount();

            var server = await Container.GetPlexServerService.GetServerAsync(1);

            var plexLibrary =
                await Container.GetPlexLibraryService.GetPlexLibraryAsync(server.Value.PlexLibraries
                    .FirstOrDefault(x => x.Name == "Test Media Movies").Id);

            var downloadTasksResult = await Container.GetPlexDownloadTaskFactory.GenerateAsync(
                new List<int> { plexLibrary.Value.Movies.First().Id }, PlexMediaType.Movie);

            downloadTasksResult.Value.First().Id = 1;

            //  // Act
            var downloadClient = Container.GetPlexDownloadClientFactory(downloadTasksResult.Value.First());

            await downloadClient.Start();

            await Task.Delay(30000);

            // var result = await downloadManager.CreateDownload(downloadTask);
            //
            //  // Assert
            //  result.IsFailed.ShouldBeFalse();
            //  result.Value.ShouldBeTrue();
        }
    }
}