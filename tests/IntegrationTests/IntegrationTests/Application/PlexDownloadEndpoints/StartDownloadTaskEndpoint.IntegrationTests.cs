using System.ComponentModel;
using Autofac;
using DownloadConfiguration = Downloader.DownloadConfiguration;
using DownloadPackage = Downloader.DownloadPackage;
using DownloadProgressChangedEventArgs = Downloader.DownloadProgressChangedEventArgs;
using IDownloadService = Downloader.IDownloadService;

namespace Reaparr.IntegrationTests;

public class StartDownloadTaskEndpointIntegrationTests : BaseIntegrationTests
{
    [Test]
    public async Task ShouldSetServerUnreachable_WhenDirectDownloadFailsMidDownload()
    {
        // Arrange
        var seed = new Seed(8933);
        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexMovieLibraryCount = 1;
                    x.MovieCount = 1;
                    x.MovieDownloadTasksCount = 1;
                };

                config.OverrideServices = builder =>
                {
                    builder
                        .Register(_ =>
                            new FakeCommandExecutor().Intercept<GetDirectDownloadUrlCommand, Result<string>>(
                                (_, _) => Task.FromResult(Result.Ok("http://mock/direct-file.mkv"))
                            )
                        )
                        .As<ICommandExecutor>()
                        .InstancePerDependency();

                    builder
                        .Register(_ =>
                            (Func<DownloadConfiguration, IDownloadService>)(
                                _ => BuildMidDownloadTimeoutFailureDownloadService()
                            )
                        )
                        .SingleInstance();
                };
            }
        );

        var downloadTasks = await container.DbContext.GetAllDownloadTasksByServerAsync(
            cancellationToken: CancellationToken
        );
        downloadTasks.Count.ShouldBe(1);
        var downloadTask = downloadTasks[0].Children.FirstOrDefault();
        downloadTask.ShouldNotBeNull();

        // Act
        var client = container.GetApiClient();
        await client.SignIn();

        var testResult = await client.PUTAsync<
            StartDownloadTaskEndpoint,
            StartDownloadTaskEndpointRequest,
            BaseResultDTO
        >(new StartDownloadTaskEndpointRequest(downloadTask.Id));

        // Assert
        testResult.Response.IsSuccessStatusCode.ShouldBeTrue(
            await testResult.Response.Content.ReadAsStringAsync(CancellationToken)
        );
        testResult.Result.IsSuccess.ShouldBeTrue();

        await WaitForDatabaseConditionAsync(
            async () =>
            {
                var dbTask = await container.DbContext.GetDownloadTaskAsync(
                    downloadTask.Id,
                    cancellationToken: CancellationToken
                );

                if (dbTask?.DownloadStatus != DownloadStatus.ServerUnreachable)
                    return false;

                return await container.DbContext.DownloadTaskMovieFileLogs.AnyAsync(
                    x =>
                        x.DownloadTaskFileId == downloadTask.Id
                        && x.Status == DownloadStatus.ServerUnreachable
                        && x.LogLevel == NotificationLevel.Error
                        && x.Message.Contains("timed out while downloading"),
                    CancellationToken
                );
            },
            maxRetries: 60,
            delayMs: 250
        );

        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(
            downloadTask.Id,
            cancellationToken: CancellationToken
        );
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.ServerUnreachable);

        var logs = await container
            .DbContext.DownloadTaskMovieFileLogs.Where(x => x.DownloadTaskFileId == downloadTask.Id)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(CancellationToken);

        logs.Any(x => x.Status == DownloadStatus.Downloading).ShouldBeTrue();
        logs.Any(x => x.Status == DownloadStatus.ServerUnreachable).ShouldBeTrue();

        var serverUnreachableLogs = logs.Where(x => x.Status == DownloadStatus.ServerUnreachable).ToList();
        serverUnreachableLogs.ShouldContain(x =>
            x.LogLevel == NotificationLevel.Error && x.Message.Contains("timed out while downloading")
        );
    }

    [Test]
    public async Task ShouldStartQueuedMovieDownloadTaskOnStartCommand_WhenNoTasksAreDownloading()
    {
        // Arrange
        var seed = new Seed(8932);
        using var container = await CreateContainer(
            seed,
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
        var downloadTask = downloadTasks.First().Children.FirstOrDefault();
        downloadTask.ShouldNotBeNull();

        // Act
        var client = container.GetApiClient();
        await client.SignIn();

        var testResult = await client.PUTAsync<
            StartDownloadTaskEndpoint,
            StartDownloadTaskEndpointRequest,
            BaseResultDTO
        >(new StartDownloadTaskEndpointRequest(downloadTask.Id));
        testResult.Response.IsSuccessStatusCode.ShouldBeTrue(
            await testResult.Response.Content.ReadAsStringAsync(CancellationToken)
        );

        var finalDownload = await container.WaitForDownloadStatusAsync(
            downloadTask.Id,
            [DownloadStatus.Completed],
            TimeSpan.FromSeconds(20),
            CancellationToken
        );

        // Assert
        var result = testResult.Result;
        result.IsSuccess.ShouldBeTrue();
        finalDownload.ShouldNotBeNull(
            $"WaitForDownloadStatusAsync timed out waiting for download '{downloadTask.Id}' to reach status '{DownloadStatus.Completed}'."
        );

        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(
            downloadTask.Id,
            cancellationToken: CancellationToken
        );
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);
    }

    private static IDownloadService BuildMidDownloadTimeoutFailureDownloadService()
    {
        var package = new DownloadPackage
        {
            FileName = "mid-download-timeout.mkv",
            TotalFileSize = 5 * 1024 * 1024,
            Urls = ["http://mock/mid-download-timeout.mkv"],
        };

        var mock = new Mock<IDownloadService>();
        mock.Setup(x => x.Clear()).Returns(Task.CompletedTask);
        mock.Setup(x => x.CancelTaskAsync()).Returns(Task.CompletedTask);
        mock.Setup(x => x.Package).Returns(package);

        mock.Setup(x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>(
                async (_, _, cancellationToken) =>
                {
                    mock.Raise(
                        x => x.DownloadProgressChanged += null,
                        mock.Object,
                        new DownloadProgressChangedEventArgs("Main")
                        {
                            TotalBytesToReceive = package.TotalFileSize,
                            ReceivedBytesSize = 2 * 1024 * 1024,
                            BytesPerSecondSpeed = 512 * 1024,
                        }
                    );

                    await Task.Delay(TimeSpan.FromMilliseconds(1200), cancellationToken);

                    mock.Raise(
                        x => x.DownloadFileCompleted += null,
                        mock.Object,
                        new AsyncCompletedEventArgs(new TimeoutException("timed out while downloading"), false, package)
                    );
                }
            );

        return mock.Object;
    }
}
