using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;

namespace Reaparr.Data.UnitTests;

public class DbContextExtensionsDownloadTasksUnitTests : BaseUnitTest
{
    public DbContextExtensionsDownloadTasksUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSetTheDownloadTaskParentOfTypeMovieDataToDownloadFinished_WhenTheMovieDataIsDownloadStatusIsDownloadFinished()
    {
        // Arrange
        await SetupDatabase(
            77674,
            config =>
            {
                config.MovieDownloadTasksCount = 5;
            }
        );

        var downloadTasks = await IDbContext.DownloadTaskMovie.Include(x => x.Children).ToListAsync(CancellationToken);
        var testDownloadTask = downloadTasks.First().Children.First();
        await IDbContext.SetDownloadStatus(testDownloadTask.ToKey(), DownloadStatus.DownloadFinished);

        // Act
        await IDbContext.DetermineDownloadStatus(testDownloadTask.ToKey(), CancellationToken);

        // Assert
        downloadTasks = await IDbContext.DownloadTaskMovie.Include(x => x.Children).ToListAsync(CancellationToken);

        downloadTasks[0].DownloadStatus.ShouldBe(DownloadStatus.DownloadFinished);
    }

    [Fact]
    public async Task ShouldSetTheDownloadTaskParentOfTypeEpisodeDataToError_WhenTheEpisodeDataIsDownloadStatusIsError()
    {
        // Arrange
        await SetupDatabase(
            864828,
            config =>
            {
                config.TvShowDownloadTasksCount = 5;
                config.TvShowSeasonDownloadTasksCount = 5;
                config.TvShowEpisodeDownloadTasksCount = 5;
            }
        );

        var downloadTasks = await IDbContext.DownloadTaskTvShow.IncludeAll().ToListAsync(CancellationToken);

        var downloadTaskTvShowEpisodeFile = downloadTasks
            .ElementAt(3)
            .Children.ElementAt(2)
            .Children.ElementAt(3)
            .Children.ElementAt(0);

        await IDbContext.SetDownloadStatus(downloadTaskTvShowEpisodeFile.ToKey(), DownloadStatus.Error);

        // Act
        await IDbContext.DetermineDownloadStatus(downloadTaskTvShowEpisodeFile.ToKey(), CancellationToken);

        // Assert
        var downloadTasksDb = await IDbContext
            .DownloadTaskTvShow.AsTracking()
            .IncludeAll()
            .ToListAsync(CancellationToken);
        downloadTasksDb[3].DownloadStatus.ShouldBe(DownloadStatus.Error);
    }

    [Fact]
    public async Task ShouldReturnDownloadTaskTypeMovie_WhenTheGuidIsOfTypeDownloadTaskMovie()
    {
        // Arrange
        await SetupDatabase(81434, config => config.MovieDownloadTasksCount = 5);
        var downloadTasks = await IDbContext.DownloadTaskMovie.ToListAsync(CancellationToken);
        var testDownloadTask = downloadTasks[2];

        // Act
        var downloadTaskType = await IDbContext.GetDownloadTaskTypeAsync(testDownloadTask.Id, CancellationToken);

        // Assert
        downloadTaskType.ShouldBe(DownloadTaskType.Movie);
    }

    [Fact]
    public async Task ShouldReturnDownloadTaskTypeTvShow_WhenTheGuidIsOfTypeDownloadTaskTvShow()
    {
        // Arrange
        await SetupDatabase(91671, config => config.TvShowDownloadTasksCount = 2);
        var downloadTasks = await IDbContext.DownloadTaskTvShow.ToListAsync(CancellationToken);
        var testDownloadTask = downloadTasks[1];

        // Act
        var downloadTaskType = await IDbContext.GetDownloadTaskTypeAsync(testDownloadTask.Id, CancellationToken);

        // Assert
        downloadTaskType.ShouldBe(DownloadTaskType.TvShow);
    }

    [Fact]
    public async Task ShouldReturnDownloadTaskTypeSeason_WhenTheGuidIsOfTypeDownloadTaskTvShowSeason()
    {
        // Arrange
        await SetupDatabase(48398, config => config.TvShowDownloadTasksCount = 3);
        var downloadTasks = await IDbContext.DownloadTaskTvShowSeason.ToListAsync(CancellationToken);
        var testDownloadTask = downloadTasks[2];

        // Act
        var downloadTaskType = await IDbContext.GetDownloadTaskTypeAsync(testDownloadTask.Id, CancellationToken);

        // Assert
        downloadTaskType.ShouldBe(DownloadTaskType.Season);
    }

    [Fact]
    public async Task ShouldReturnDownloadTaskTypeEpisode_WhenTheGuidIsOfTypeDownloadTaskTvShowEpisode()
    {
        // Arrange
        await SetupDatabase(74950, config => config.TvShowDownloadTasksCount = 2);
        var downloadTasks = await IDbContext.DownloadTaskTvShowEpisode.ToListAsync(CancellationToken);
        var testDownloadTask = downloadTasks[0];

        // Act
        var downloadTaskType = await IDbContext.GetDownloadTaskTypeAsync(testDownloadTask.Id, CancellationToken);

        // Assert
        downloadTaskType.ShouldBe(DownloadTaskType.Episode);
    }

    [Theory]
    [InlineData(DownloadTaskType.Movie)]
    [InlineData(DownloadTaskType.TvShow)]
    [InlineData(DownloadTaskType.Season)]
    [InlineData(DownloadTaskType.Episode)]
    public async Task ShouldReturnError_WhenUnsupportedDownloadTaskTypeProvided(DownloadTaskType type)
    {
        // Arrange
        await SetupDatabase(
            81238,
            config =>
            {
                config.MovieDownloadTasksCount = 5;
                config.TvShowDownloadTasksCount = 5;
                config.TvShowSeasonDownloadTasksCount = 5;
                config.TvShowEpisodeDownloadTasksCount = 5;
            }
        );

        var dbContext = IDbContext;
        DownloadTaskKey? key;
        switch (type)
        {
            case DownloadTaskType.Movie:
                key = await dbContext.DownloadTaskMovie.ProjectToKey().FirstOrDefaultAsync(CancellationToken);
                break;
            case DownloadTaskType.TvShow:
                key = await dbContext.DownloadTaskTvShow.ProjectToKey().FirstOrDefaultAsync(CancellationToken);
                break;
            case DownloadTaskType.Season:
                key = await dbContext.DownloadTaskTvShowSeason.ProjectToKey().FirstOrDefaultAsync(CancellationToken);
                break;
            case DownloadTaskType.Episode:
                key = await dbContext.DownloadTaskTvShowEpisode.ProjectToKey().FirstOrDefaultAsync(CancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        key.ShouldNotBeNull();

        // Act
        var result = await dbContext.ResetDownloadTaskProgress(key, DownloadStatus.Stopped, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Message.Contains("not supported"));
    }

    [Fact]
    public async Task ShouldReturnResetDownloadTaskTypeMovieFile_WhenResetDownloadTaskProgressCalled()
    {
        // Arrange
        await SetupDatabase(43481, config => config.MovieDownloadTasksCount = 5);
        var dbContext = IDbContext;
        var downloadTasks = await dbContext.DownloadTaskMovieFile.AsTracking().ToListAsync(CancellationToken);
        var testDownloadTask = downloadTasks[2];

        testDownloadTask.DataTotal = 5000;
        testDownloadTask.DownloadSpeed = 50;
        testDownloadTask.DataReceived = 50;
        testDownloadTask.FileTransferSpeed = 50;
        testDownloadTask.FileDataTransferred = 50;
        testDownloadTask.CurrentFileTransferBytesOffset = 50;
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var resetResult = await IDbContext.ResetDownloadTaskProgress(
            testDownloadTask.ToKey(),
            DownloadStatus.Stopped,
            CancellationToken
        );

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
        var downloadTaskDb = await dbContext.GetDownloadTaskFileAsync(testDownloadTask.ToKey(), CancellationToken);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadSpeed.ShouldBe(0);
        downloadTaskDb.DataReceived.ShouldBe(0);
        downloadTaskDb.FileTransferSpeed.ShouldBe(0);
        downloadTaskDb.FileDataTransferred.ShouldBe(0);
        downloadTaskDb.CurrentFileTransferBytesOffset.ShouldBe(0);
        downloadTaskDb.DataTotal.ShouldBe(5000);
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Stopped);

        downloadTaskDb.FileName.ShouldBe(testDownloadTask.FileName);
        downloadTaskDb.DownloadDirectory.ShouldBe(testDownloadTask.DownloadDirectory);
        downloadTaskDb.DestinationDirectory.ShouldBe(testDownloadTask.DestinationDirectory);
        downloadTaskDb.FileLocationUrl.ShouldBe(testDownloadTask.FileLocationUrl);
        downloadTaskDb.CreatedAt.ShouldBe(testDownloadTask.CreatedAt);
        downloadTaskDb.Quality.ShouldBe(testDownloadTask.Quality);
        downloadTaskDb.IsDownloadable.ShouldBe(testDownloadTask.IsDownloadable);
        downloadTaskDb.MediaType.ShouldBe(testDownloadTask.MediaType);
        downloadTaskDb.FullTitle.ShouldBe(testDownloadTask.FullTitle);
        downloadTaskDb.PlexServerId.ShouldBe(testDownloadTask.PlexServerId);
        downloadTaskDb.PlexLibraryId.ShouldBe(testDownloadTask.PlexLibraryId);
    }

    [Fact]
    public async Task ShouldReturnResetDownloadTaskTypeEpisodeFile_WhenResetDownloadTaskProgressCalled()
    {
        // Arrange
        await SetupDatabase(
            8231434,
            config =>
            {
                config.TvShowDownloadTasksCount = 2;
                config.TvShowSeasonDownloadTasksCount = 2;
                config.TvShowEpisodeDownloadTasksCount = 2;
            }
        );
        var dbContext = IDbContext;
        var downloadTasks = await dbContext.DownloadTaskTvShowEpisodeFile.AsTracking().ToListAsync(CancellationToken);
        var testDownloadTask = downloadTasks[4];

        testDownloadTask.DataTotal = 5000;
        testDownloadTask.DownloadSpeed = 50;
        testDownloadTask.DataReceived = 50;
        testDownloadTask.FileTransferSpeed = 50;
        testDownloadTask.FileDataTransferred = 50;
        testDownloadTask.CurrentFileTransferBytesOffset = 50;
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var resetResult = await IDbContext.ResetDownloadTaskProgress(
            testDownloadTask.ToKey(),
            DownloadStatus.Stopped,
            CancellationToken
        );

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
        var downloadTaskDb = await dbContext.GetDownloadTaskFileAsync(testDownloadTask.ToKey(), CancellationToken);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadSpeed.ShouldBe(0);
        downloadTaskDb.DataReceived.ShouldBe(0);
        downloadTaskDb.FileTransferSpeed.ShouldBe(0);
        downloadTaskDb.FileDataTransferred.ShouldBe(0);
        downloadTaskDb.CurrentFileTransferBytesOffset.ShouldBe(0);
        downloadTaskDb.DataTotal.ShouldBe(5000);

        downloadTaskDb.FileName.ShouldBe(testDownloadTask.FileName);
        downloadTaskDb.DownloadDirectory.ShouldBe(testDownloadTask.DownloadDirectory);
        downloadTaskDb.DestinationDirectory.ShouldBe(testDownloadTask.DestinationDirectory);
        downloadTaskDb.FileLocationUrl.ShouldBe(testDownloadTask.FileLocationUrl);
        downloadTaskDb.CreatedAt.ShouldBe(testDownloadTask.CreatedAt);
        downloadTaskDb.Quality.ShouldBe(testDownloadTask.Quality);
        downloadTaskDb.IsDownloadable.ShouldBe(testDownloadTask.IsDownloadable);
        downloadTaskDb.MediaType.ShouldBe(testDownloadTask.MediaType);
        downloadTaskDb.FullTitle.ShouldBe(testDownloadTask.FullTitle);
        downloadTaskDb.PlexServerId.ShouldBe(testDownloadTask.PlexServerId);
        downloadTaskDb.PlexLibraryId.ShouldBe(testDownloadTask.PlexLibraryId);
    }

    [Fact]
    public async Task ShouldBuildDownloadUrl_WithHttpsConnectionAndToken()
    {
        // Arrange
        var seed = await SetupDatabase(
            561231,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexServerConnectionPerServerCount = 0;
                config.PlexAccountCount = 1;
            }
        );

        var db = IDbContext;
        var server = await db.PlexServers.FirstAsync(CancellationToken);

        // Create an HTTPS online connection
        var conn = FakeData.GetPlexServerConnections(seed).Generate();
        conn = new PlexServerConnection
        {
            Protocol = "https",
            Address = conn.Address,
            Port = conn.Port,
            Url = $"https://{conn.Address}:{conn.Port}",
            Local = false,
            Relay = false,
            IPv4 = true,
            IPv6 = false,
            IsCustom = false,
            PlexServerId = server.Id,
        };
        conn.LatestConnectionStatus = FakeData.GetPlexServerStatus(seed).Generate();
        conn.LatestConnectionStatus.PlexServerId = server.Id;
        conn.LatestConnectionStatus.PlexServerConnectionId = conn.Id;
        db.PlexServerConnections.Add(conn);
        await db.SaveChangesAsync(CancellationToken);

        var access = await db.PlexAccountServers.FirstAsync(CancellationToken);
        var fileLocationUrl = "/library/parts/123/file.mkv";

        // Act
        var result = await db.GetDownloadUrl(server.Id, fileLocationUrl, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe($"{conn.Url}{fileLocationUrl}?X-Plex-Token={access.AuthToken}");
    }

    [Fact]
    public async Task ShouldFail_GetDownloadUrl_WhenNoConnectionsAvailable()
    {
        // Arrange
        await SetupDatabase(
            992341,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexServerConnectionPerServerCount = 0;
                config.PlexAccountCount = 1;
            }
        );
        var db = IDbContext;
        var server = await db.PlexServers.FirstAsync(CancellationToken);

        // Act
        var result = await db.GetDownloadUrl(server.Id, "/file", CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldFail_GetDownloadUrl_WhenNoTokenAvailable()
    {
        // Arrange
        var seed = await SetupDatabase(
            335522,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexServerConnectionPerServerCount = 0;
                config.PlexAccountCount = 0;
            }
        );

        var db = IDbContext;
        var server = await db.PlexServers.FirstAsync(CancellationToken);
        var conn = FakeData.GetPlexServerConnections(seed).Generate();
        conn = new PlexServerConnection
        {
            Protocol = "http",
            Address = conn.Address,
            Port = conn.Port,
            Url = $"http://{conn.Address}:{conn.Port}",
            Local = false,
            Relay = false,
            IPv4 = true,
            IPv6 = false,
            IsCustom = false,
            PlexServerId = server.Id,
        };
        conn.LatestConnectionStatus = FakeData.GetPlexServerStatus(seed).Generate();
        conn.LatestConnectionStatus.PlexServerId = server.Id;
        conn.LatestConnectionStatus.PlexServerConnectionId = conn.Id;
        db.PlexServerConnections.Add(conn);
        await db.SaveChangesAsync(CancellationToken);

        // Act
        var result = await db.GetDownloadUrl(server.Id, "/file", CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnNullDownloadTaskKey_WhenGuidEmpty()
    {
        // Arrange
        await SetupDatabase(114455);

        // Act
        var key = await IDbContext.GetDownloadTaskKeyAsync(Guid.Empty, CancellationToken);

        // Assert
        key.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldReturnDownloadTask_WhenTypeIsNoneAndIdMatchesMovieFile()
    {
        // Arrange
        await SetupDatabase(
            226677,
            config =>
            {
                config.MovieDownloadTasksCount = 3;
            }
        );
        var db = IDbContext;
        var movieFile = await db.DownloadTaskMovieFile.FirstAsync(CancellationToken);

        // Act
        var generic = await db.GetDownloadTaskAsync(movieFile.Id, DownloadTaskType.None, CancellationToken);

        // Assert
        generic.ShouldNotBeNull();
        generic.Id.ShouldBe(movieFile.Id);
        generic.DownloadTaskType.ShouldBe(DownloadTaskType.MovieData);
        generic.PlexServer.ShouldNotBeNull();
        generic.PlexLibrary.ShouldNotBeNull();
    }
}
