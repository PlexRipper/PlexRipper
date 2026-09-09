using Reaparr.Application.Contracts;

namespace Reaparr.Data.UnitTests;

public class DbContextExtensionsDownloadTasksUnitTests : BaseUnitTest
{
    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
    [Arguments(DownloadTaskType.Movie)]
    [Arguments(DownloadTaskType.TvShow)]
    [Arguments(DownloadTaskType.Season)]
    [Arguments(DownloadTaskType.Episode)]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
    public async Task ShouldReturnNullDownloadTaskKey_WhenGuidEmpty()
    {
        // Arrange
        await SetupDatabase(114455);

        // Act
        var key = await IDbContext.GetDownloadTaskKeyAsync(Guid.Empty, CancellationToken);

        // Assert
        key.ShouldBeNull();
    }

    [Test]
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

    [Test]
    public async Task ShouldReturnMovieRoot_WhenRadarrOwnedMovieFileHasNoIntegrationFilter()
    {
        // Arrange
        await SetupDatabase(
            226678,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
                config.RadarrIntegrationCount = 1;
                config.AssignUnownedDownloadTasksToRadarrIntegration = true;
            }
        );
        var dbContext = IDbContext;
        var movie = await dbContext.DownloadTaskMovie.SingleAsync(CancellationToken);
        var movieFile = await dbContext.DownloadTaskMovieFile.SingleAsync(CancellationToken);

        // Act
        var rootKey = await dbContext.GetRootDownloadTaskKeyAsync(
            movieFile.ToKey(),
            cancellationToken: CancellationToken
        );

        // Assert
        movieFile.RadarrIntegrationId.ShouldNotBeNull();
        rootKey.ShouldNotBeNull();
        rootKey.Id.ShouldBe(movie.Id);
        rootKey.Type.ShouldBe(DownloadTaskType.Movie);
        rootKey.PlexServerId.ShouldBe(movieFile.PlexServerId);
        rootKey.PlexLibraryId.ShouldBe(movieFile.PlexLibraryId);
    }

    [Test]
    public async Task ShouldReturnTvShowRoot_WhenRadarrOwnedEpisodeFileHasNoIntegrationFilter()
    {
        // Arrange
        await SetupDatabase(
            226679,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.RadarrIntegrationCount = 1;
                config.AssignUnownedDownloadTasksToRadarrIntegration = true;
            }
        );
        var dbContext = IDbContext;
        var tvShow = await dbContext.DownloadTaskTvShow.SingleAsync(CancellationToken);
        var episodeFile = await dbContext.DownloadTaskTvShowEpisodeFile.SingleAsync(CancellationToken);

        // Act
        var rootKey = await dbContext.GetRootDownloadTaskKeyAsync(
            episodeFile.ToKey(),
            cancellationToken: CancellationToken
        );

        // Assert
        episodeFile.RadarrIntegrationId.ShouldNotBeNull();
        rootKey.ShouldNotBeNull();
        rootKey.Id.ShouldBe(tvShow.Id);
        rootKey.Type.ShouldBe(DownloadTaskType.TvShow);
        rootKey.PlexServerId.ShouldBe(episodeFile.PlexServerId);
        rootKey.PlexLibraryId.ShouldBe(episodeFile.PlexLibraryId);
    }

    [Test]
    public async Task ShouldReturnNull_WhenRadarrOwnedMovieFileHasDifferentIntegrationFilter()
    {
        // Arrange
        await SetupDatabase(
            226680,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
                config.RadarrIntegrationCount = 1;
                config.AssignUnownedDownloadTasksToRadarrIntegration = true;
            }
        );
        var dbContext = IDbContext;
        var movieFile = await dbContext.DownloadTaskMovieFile.SingleAsync(CancellationToken);
        var differentIntegration = new IntegrationIdentity(
            IntegrationType.Radarr,
            Guid.Parse("3ad7b0be-b78e-43f2-ab07-5a24649667f3")
        );

        // Act
        var rootKey = await dbContext.GetRootDownloadTaskKeyAsync(
            movieFile.ToKey(),
            differentIntegration,
            CancellationToken
        );

        // Assert
        movieFile.RadarrIntegrationId.ShouldNotBeNull();
        movieFile.RadarrIntegrationId.ShouldNotBe(differentIntegration.Id);
        rootKey.ShouldBeNull();
    }

    [Test]
    public async Task ShouldReturnOwnedAndUnownedTasks_WhenIntegrationFilterIsNull()
    {
        // Arrange
        await SetupDatabase(
            226681,
            config =>
            {
                config.MovieDownloadTasksCount = 2;
                config.RadarrIntegrationCount = 1;
            }
        );
        var dbContext = IDbContext;
        var integrationId = await dbContext.RadarrIntegrations.Select(x => x.Id).SingleAsync(CancellationToken);
        var movieFiles = await dbContext.DownloadTaskMovieFile.AsTracking().OrderBy(x => x.Id).ToListAsync(CancellationToken);
        movieFiles[0].RadarrIntegrationId = integrationId;
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var result = await dbContext.DownloadTaskMovieFile.WhereIntegrationIs(null).Select(x => x.Id).ToListAsync(CancellationToken);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(movieFiles[0].Id);
        result.ShouldContain(movieFiles[1].Id);
    }

    [Test]
    public async Task ShouldReturnOnlyUnownedTasks_WhenOwnershipMatchIsNull()
    {
        // Arrange
        await SetupDatabase(
            226682,
            config =>
            {
                config.MovieDownloadTasksCount = 2;
                config.RadarrIntegrationCount = 1;
            }
        );
        var dbContext = IDbContext;
        var integrationId = await dbContext.RadarrIntegrations.Select(x => x.Id).SingleAsync(CancellationToken);
        var movieFiles = await dbContext.DownloadTaskMovieFile.AsTracking().OrderBy(x => x.Id).ToListAsync(CancellationToken);
        movieFiles[0].RadarrIntegrationId = integrationId;
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var result = await dbContext.DownloadTaskMovieFile.WhereIntegrationOwnershipMatches(null).SingleAsync(CancellationToken);

        // Assert
        result.Id.ShouldBe(movieFiles[1].Id);
        result.SonarrIntegrationId.ShouldBeNull();
        result.RadarrIntegrationId.ShouldBeNull();
    }

    [Test]
    public async Task ShouldReturnOnlyMatchingRadarrTasks_WhenIntegrationIsSpecified()
    {
        // Arrange
        await SetupDatabase(
            226683,
            config =>
            {
                config.MovieDownloadTasksCount = 4;
                config.RadarrIntegrationCount = 2;
                config.SonarrIntegrationCount = 1;
            }
        );
        var dbContext = IDbContext;
        var radarrIds = await dbContext
            .RadarrIntegrations.OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);
        var sonarrId = await dbContext
            .SonarrIntegrations.Select(x => x.Id)
            .SingleAsync(CancellationToken);
        var movieFiles = await dbContext
            .DownloadTaskMovieFile.AsTracking()
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);
        movieFiles[0].RadarrIntegrationId = radarrIds[0];
        movieFiles[1].RadarrIntegrationId = radarrIds[1];
        movieFiles[2].SonarrIntegrationId = sonarrId;
        await dbContext.SaveChangesAsync(CancellationToken);
        var identity = new IntegrationIdentity(IntegrationType.Radarr, radarrIds[0]);

        // Act
        var result = await dbContext
            .DownloadTaskMovieFile.WhereIntegrationIs(identity)
            .SingleAsync(CancellationToken);

        // Assert
        result.Id.ShouldBe(movieFiles[0].Id);
        result.RadarrIntegrationId.ShouldBe(radarrIds[0]);
        result.SonarrIntegrationId.ShouldBeNull();
    }

    [Test]
    public async Task ShouldReturnOnlyMatchingSonarrTasks_WhenIntegrationIsSpecified()
    {
        // Arrange
        await SetupDatabase(
            226684,
            config =>
            {
                config.MovieDownloadTasksCount = 3;
                config.RadarrIntegrationCount = 1;
                config.SonarrIntegrationCount = 1;
            }
        );
        var dbContext = IDbContext;
        var radarrId = await dbContext
            .RadarrIntegrations.Select(x => x.Id)
            .SingleAsync(CancellationToken);
        var sonarrId = await dbContext
            .SonarrIntegrations.Select(x => x.Id)
            .SingleAsync(CancellationToken);
        var movieFiles = await dbContext
            .DownloadTaskMovieFile.AsTracking()
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);
        movieFiles[0].RadarrIntegrationId = radarrId;
        movieFiles[1].SonarrIntegrationId = sonarrId;
        await dbContext.SaveChangesAsync(CancellationToken);
        var identity = new IntegrationIdentity(IntegrationType.Sonarr, sonarrId);

        // Act
        var result = await dbContext
            .DownloadTaskMovieFile.WhereIntegrationIs(identity)
            .SingleAsync(CancellationToken);

        // Assert
        result.Id.ShouldBe(movieFiles[1].Id);
        result.SonarrIntegrationId.ShouldBe(sonarrId);
        result.RadarrIntegrationId.ShouldBeNull();
    }

    [Test]
    public async Task ShouldReturnMatchingAndUnownedTasks_WhenIntegrationOrUnownedIsRequested()
    {
        // Arrange
        await SetupDatabase(
            226685,
            config =>
            {
                config.MovieDownloadTasksCount = 4;
                config.RadarrIntegrationCount = 2;
                config.SonarrIntegrationCount = 1;
            }
        );
        var dbContext = IDbContext;
        var radarrIds = await dbContext
            .RadarrIntegrations.OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);
        var sonarrId = await dbContext
            .SonarrIntegrations.Select(x => x.Id)
            .SingleAsync(CancellationToken);
        var movieFiles = await dbContext
            .DownloadTaskMovieFile.AsTracking()
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);
        movieFiles[0].RadarrIntegrationId = radarrIds[0];
        movieFiles[1].RadarrIntegrationId = radarrIds[1];
        movieFiles[2].SonarrIntegrationId = sonarrId;
        await dbContext.SaveChangesAsync(CancellationToken);
        var identity = new IntegrationIdentity(IntegrationType.Radarr, radarrIds[0]);

        // Act
        var result = await dbContext
            .DownloadTaskMovieFile.WhereIntegrationIsOrUnowned(identity)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(movieFiles[0].Id);
        result.ShouldContain(movieFiles[3].Id);
        result.ShouldNotContain(movieFiles[1].Id);
        result.ShouldNotContain(movieFiles[2].Id);
    }

    [Test]
    public async Task ShouldNotMatchRadarrTask_WhenSonarrIdentityUsesSameId()
    {
        // Arrange
        await SetupDatabase(
            226686,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
                config.RadarrIntegrationCount = 1;
                config.AssignUnownedDownloadTasksToRadarrIntegration = true;
            }
        );
        var dbContext = IDbContext;
        var movieFile = await dbContext.DownloadTaskMovieFile.SingleAsync(CancellationToken);
        var radarrId = movieFile.RadarrIntegrationId.ShouldNotBeNull();
        var wrongTypeIdentity = new IntegrationIdentity(IntegrationType.Sonarr, radarrId);

        // Act
        var rootKey = await dbContext.GetRootDownloadTaskKeyAsync(
            movieFile.ToKey(),
            wrongTypeIdentity,
            CancellationToken
        );

        // Assert
        movieFile.SonarrIntegrationId.ShouldBeNull();
        movieFile.RadarrIntegrationId.ShouldBe(radarrId);
        rootKey.ShouldBeNull();
    }
}
