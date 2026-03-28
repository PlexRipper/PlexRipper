using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.IntegrationTests;

public class RestartDownloadTaskEndpointIntegrationTests : BaseIntegrationTests
{
    [Test]
    public async Task ShouldRestartCompletedMovieDownloadTaskOnRestartCommand_WhenTaskIsDoneDownloading()
    {
        // Arrange
        var seed = new Seed(5594564);
        using var container = await CreateContainer(
            5594564,
            config =>
            {
                config.HttpClientOptions = (x, _) =>
                {
                    x.SetupIdentityRequest(seed);
                };

                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexMovieLibraryCount = 2;
                    x.MovieCount = 10;
                    x.MovieDownloadTasksCount = 1;
                };

                config.FileSystemOptions = (system, dbContext) =>
                {
                    var downloadTask = dbContext.DownloadTaskMovieFile.First();
                    downloadTask.DownloadFilePath.ShouldNotBeNullOrEmpty();

                    var directoryPath = system.Path.GetDirectoryName(downloadTask.DownloadFilePath);
                    directoryPath.ShouldNotBeNullOrEmpty();
                    system.Directory.CreateDirectory(directoryPath);
                    system.File.WriteAllBytes(downloadTask.DownloadFilePath, FakeData.GetDownloadFile(10.0 / 4.0));
                };
            }
        );

        await container.DbContext.PlexServerConnections.ExecuteUpdateAsync(
            x => x.SetProperty(y => y.Url, _ => "https://download.blender.org"),
            CancellationToken
        );
        await container.DbContext.DownloadTaskMovieFile.ExecuteUpdateAsync(
            x => x.SetProperty(y => y.FileLocationUrl, _ => "/peach/bigbuckbunny_movies/BigBuckBunny_320x180.mp4"),
            CancellationToken
        );

        var downloadTasks = await container.DbContext.GetAllDownloadTasksByServerAsync(
            cancellationToken: CancellationToken
        );
        downloadTasks.Count.ShouldBe(1);
        var downloadTask = downloadTasks[0].Children[0];

        await container.DbContext.SetDownloadStatus(downloadTask.ToKey(), DownloadStatus.Completed);

        // Act
        var client = container.GetApiClient();
        await client.SignIn();
        var testResult = await client.PUTAsync<
            RestartDownloadTaskEndpoint,
            RestartDownloadTaskEndpointRequest,
            BaseResultDTO
        >(new RestartDownloadTaskEndpointRequest(downloadTask.Id));
        testResult.Response.IsSuccessStatusCode.ShouldBeTrue(
            await testResult.Response.Content.ReadAsStringAsync(CancellationToken)
        );

        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(
            downloadTask.Id,
            cancellationToken: CancellationToken
        );
        downloadTaskDb.ShouldNotBeNull();

        downloadTaskDb.DownloadStatus.ShouldBeOneOf(DownloadStatus.Queued, DownloadStatus.Completed);

        var finalDownload = await container.WaitForDownloadStatusAsync(
            downloadTask.Id,
            [DownloadStatus.Completed],
            TimeSpan.FromSeconds(20),
            CancellationToken
        );

        // Assert
        var result = testResult.Result;
        result.IsSuccess.ShouldBeTrue();
        finalDownload.ShouldNotBeNull();

        downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(
            downloadTask.Id,
            cancellationToken: CancellationToken
        );
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);
    }
}
