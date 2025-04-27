using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application.UnitTests;

public class RefreshLibraryMediaCommand_UnitTests : BaseUnitTest<RefreshLibraryMediaCommandHandler>
{
    public RefreshLibraryMediaCommand_UnitTests(ITestOutputHelper output)
        : base(output) { }

    private async Task<PlexLibrary> GetUpdatedLibrary(Seed seed, PlexMediaType type)
    {
        var plexLibrary = await IDbContext.PlexLibraries.Where(x => x.Type == type).FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        var fakeLibrary = FakeData.GetPlexLibrary(seed, type).Generate();
        var newLibrary = new PlexLibrary
        {
            Id = plexLibrary.Id,
            Key = plexLibrary.Key,
            Type = type,
            Title = fakeLibrary.Title,
            PlexServerId = plexLibrary.PlexServerId,
            PlexServer = plexLibrary.PlexServer,
            Uuid = fakeLibrary.Uuid,
            CreatedAt = fakeLibrary.CreatedAt,
            UpdatedAt = fakeLibrary.UpdatedAt,
            ScannedAt = fakeLibrary.ScannedAt,
            DefaultDestination = fakeLibrary.DefaultDestination,
            DefaultDestinationId = fakeLibrary.DefaultDestinationId,
        };

        newLibrary.Movies.AddRange(fakeLibrary.Movies);
        newLibrary.TvShows.AddRange(fakeLibrary.TvShows);
        newLibrary.PlexAccountLibraries.AddRange(fakeLibrary.PlexAccountLibraries);

        return newLibrary;
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenPlexLibraryNotFound()
    {
        // Arrange
        await SetupDatabase(
            1337,
            config =>
            {
                config.PlexMovieLibraryCount = 0;
            }
        );

        // Act
        var command = new RefreshLibraryMediaCommand(PlexLibraryId: 1, Action: _ => { });
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenSyncLibraryMediaFails()
    {
        // Arrange
        var seed = await SetupDatabase(1338, config => config.PlexMovieLibraryCount = 1);
        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        mock.SetupCommand(It.IsAny<GetLibraryMediaCommand>).ReturnsAsync(Result.Fail<LibraryMetadata>("Sync failed"));

        var command = new RefreshLibraryMediaCommand(plexLibrary.Id, _ => { });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("Sync failed"));
    }

    [Theory]
    [InlineData(PlexMediaType.Movie)]
    [InlineData(PlexMediaType.TvShow)]
    public async Task ShouldReturnOkResult_WhenLibraryIsSynced(PlexMediaType libraryType)
    {
        // Arrange
        var seed = await SetupDatabase(
            1339,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = libraryType == PlexMediaType.Movie ? 1 : 0;
                config.PlexTvShowLibraryCount = libraryType == PlexMediaType.TvShow ? 1 : 0;
            }
        );

        var updatedLibrary = await GetUpdatedLibrary(seed, libraryType);

        mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()));

        mock.SetupCommand(It.IsAny<GetLibraryMediaCommand>)
            .ReturnsAsync(
                (ICommand<Result<LibraryMetadata>> command, CancellationToken _) =>
                {
                    if (command is GetLibraryMediaCommand getLibraryMediaCommand)
                    {
                        getLibraryMediaCommand.Action?.Invoke(
                            new MediaSyncProgress
                            {
                                Type = libraryType,
                                Received = updatedLibrary.MediaCount,
                                Total = updatedLibrary.MediaCount,
                                TimeRemaining = TimeSpan.Zero,
                            }
                        );
                    }

                    return Result.Ok(new LibraryMetadata { Library = updatedLibrary });
                }
            );

        mock.SetupMediator(It.IsAny<SyncPlexLibraryMediaMetaDataCommand>).ReturnsAsync(Result.Ok());

        switch (libraryType)
        {
            case PlexMediaType.Movie:
                mock.SetupCommand(It.IsAny<RefreshPlexMovieLibraryCommand>).ReturnsAsync(Result.Ok(updatedLibrary));
                break;

            case PlexMediaType.TvShow:
                mock.SetupCommand(It.IsAny<RefreshPlexTvShowLibraryCommand>).ReturnsAsync(Result.Ok(updatedLibrary));
                break;
        }

        // Act
        var command = new RefreshLibraryMediaCommand(updatedLibrary.Id, _ => { });
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var dbLibrary = await IDbContext.PlexLibraries.Where(x => x.Id == updatedLibrary.Id).FirstOrDefaultAsync();
        dbLibrary.ShouldNotBeNull();
        dbLibrary.SyncedAt.ShouldNotBeNull();

        mock.Mock<IRefreshLibraryProgressReporter>()
            .Verify(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenSyncLibraryMediaMetadataFails()
    {
        // Arrange
        await SetupDatabase(
            1440,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 3;
            }
        );
        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()));

        mock.SetupCommand(It.IsAny<GetLibraryMediaCommand>)
            .ReturnsAsync(Result.Ok(new LibraryMetadata { Library = plexLibrary }));

        mock.SetupMediator(It.IsAny<SyncPlexLibraryMediaMetaDataCommand>)
            .ReturnsAsync(Result.Fail("Metadata sync failed"));

        // Act
        var command = new RefreshLibraryMediaCommand(plexLibrary.Id, _ => { });
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("Metadata sync failed"));
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenMovieLibraryCommandFails()
    {
        // Arrange
        var seed = await SetupDatabase(
            1441,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 3;
            }
        );
        var updatedLibrary = await GetUpdatedLibrary(seed, PlexMediaType.Movie);

        mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()));

        mock.SetupCommand(It.IsAny<GetLibraryMediaCommand>)
            .ReturnsAsync(Result.Ok(new LibraryMetadata { Library = updatedLibrary }));

        mock.SetupMediator(It.IsAny<SyncPlexLibraryMediaMetaDataCommand>).ReturnsAsync(Result.Ok());

        mock.SetupCommand(It.IsAny<RefreshPlexMovieLibraryCommand>)
            .ReturnsAsync(Result.Fail<PlexLibrary>("Movie command failed"));

        var command = new RefreshLibraryMediaCommand(updatedLibrary.Id, _ => { });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("Movie command failed"));
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenTvShowLibraryCommandFails()
    {
        // Arrange
        var seed = await SetupDatabase(
            1442,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 3;
            }
        );
        var updatedLibrary = await GetUpdatedLibrary(seed, PlexMediaType.TvShow);

        mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()));

        mock.SetupCommand(It.IsAny<GetLibraryMediaCommand>)
            .ReturnsAsync(Result.Ok(new LibraryMetadata { Library = updatedLibrary }));

        mock.SetupMediator(It.IsAny<SyncPlexLibraryMediaMetaDataCommand>).ReturnsAsync(Result.Ok());

        mock.SetupCommand(It.IsAny<RefreshPlexTvShowLibraryCommand>)
            .ReturnsAsync(Result.Fail<PlexLibrary>("TV Show command failed"));

        var command = new RefreshLibraryMediaCommand(updatedLibrary.Id, _ => { });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("TV Show command failed"));
    }
}
