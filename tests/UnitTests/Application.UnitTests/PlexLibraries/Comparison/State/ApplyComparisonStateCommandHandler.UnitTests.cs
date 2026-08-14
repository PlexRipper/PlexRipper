namespace Reaparr.Application.UnitTests;

public class ApplyComparisonStateCommandHandlerUnitTests : BaseCommandUnitTest<ApplyComparisonStateCommand>
{
    [Test]
    public async Task ShouldReturnSuccessWithoutDispatch_WhenItemsAreEmpty()
    {
        // Arrange
        await SetupDatabase(
            70,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var command = new ApplyComparisonStateCommand([], 1, PlexMediaType.Movie);

        Mock.SetupCommand<Result>(x => x is ApplyOwnedMovieComparisonStateCommand)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);
        Mock.SetupCommand<Result>(x => x is ApplyRemoteMovieComparisonStateCommand)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnSuccessWithoutDispatch_WhenPlexLibraryIdIsZero()
    {
        // Arrange
        var items = new List<PlexMediaSlimDTO> { CreateItem(881, PlexMediaType.Movie) };
        var command = new ApplyComparisonStateCommand(items, 0, PlexMediaType.Movie);

        Mock.SetupCommand<Result>(x => x is ApplyOwnedMovieComparisonStateCommand)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);
        Mock.SetupCommand<Result>(x => x is ApplyRemoteMovieComparisonStateCommand)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        items[0].ComparisonId.ShouldBe(PlexMediaComparisonState.NotCompared.ToComparisonId());
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldDispatchOwnedMovieProjection_WhenMovieLibraryIsOwned()
    {
        // Arrange
        await SetupDatabase(
            71,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var library = await IDbContext.PlexLibraries.SingleAsync(CancellationToken);
        await SetOwnedOverrideAsync(library.PlexServerId, true);
        var items = new List<PlexMediaSlimDTO> { CreateItem(882, PlexMediaType.Movie) };
        var command = new ApplyComparisonStateCommand(items, library.Id, PlexMediaType.Movie);

        Mock.SetupCommand<Result>(x =>
                (x as ApplyOwnedMovieComparisonStateCommand) != null
                && ((ApplyOwnedMovieComparisonStateCommand)x).OwnedLibraryId == library.Id
                && ReferenceEquals(((ApplyOwnedMovieComparisonStateCommand)x).Items, items)
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);
        Mock.SetupCommand<Result>(x => x is ApplyRemoteMovieComparisonStateCommand)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldDispatchRemoteMovieProjection_WhenMovieLibraryIsNotOwned()
    {
        // Arrange
        await SetupDatabase(
            72,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var library = await IDbContext.PlexLibraries.SingleAsync(CancellationToken);
        await SetOwnedOverrideAsync(library.PlexServerId, false);
        var items = new List<PlexMediaSlimDTO> { CreateItem(883, PlexMediaType.Movie) };
        var command = new ApplyComparisonStateCommand(items, library.Id, PlexMediaType.Movie);

        Mock.SetupCommand<Result>(x => x is ApplyOwnedMovieComparisonStateCommand)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);
        Mock.SetupCommand<Result>(x =>
                (x as ApplyRemoteMovieComparisonStateCommand) != null
                && ((ApplyRemoteMovieComparisonStateCommand)x).RemoteLibraryId == library.Id
                && ReferenceEquals(((ApplyRemoteMovieComparisonStateCommand)x).Items, items)
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldDispatchOwnedTvShowProjection_WhenTvShowLibraryIsOwned()
    {
        // Arrange
        await SetupDatabase(
            73,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var library = await IDbContext.PlexLibraries.SingleAsync(CancellationToken);
        await SetOwnedOverrideAsync(library.PlexServerId, true);
        var items = new List<PlexMediaSlimDTO> { CreateItem(884, PlexMediaType.TvShow) };
        var command = new ApplyComparisonStateCommand(items, library.Id, PlexMediaType.TvShow);

        Mock.SetupCommand<Result>(x =>
                (x as ApplyOwnedTvShowComparisonStateCommand) != null
                && ((ApplyOwnedTvShowComparisonStateCommand)x).OwnedLibraryId == library.Id
                && ReferenceEquals(((ApplyOwnedTvShowComparisonStateCommand)x).Items, items)
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);
        Mock.SetupCommand<Result>(x => x is ApplyRemoteTvShowComparisonStateCommand)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldDispatchRemoteTvShowProjection_WhenTvShowLibraryIsNotOwned()
    {
        // Arrange
        await SetupDatabase(
            74,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var library = await IDbContext.PlexLibraries.SingleAsync(CancellationToken);
        await SetOwnedOverrideAsync(library.PlexServerId, false);
        var items = new List<PlexMediaSlimDTO> { CreateItem(885, PlexMediaType.TvShow) };
        var command = new ApplyComparisonStateCommand(items, library.Id, PlexMediaType.TvShow);

        Mock.SetupCommand<Result>(x => x is ApplyOwnedTvShowComparisonStateCommand)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);
        Mock.SetupCommand<Result>(x =>
                (x as ApplyRemoteTvShowComparisonStateCommand) != null
                && ((ApplyRemoteTvShowComparisonStateCommand)x).RemoteLibraryId == library.Id
                && ReferenceEquals(((ApplyRemoteTvShowComparisonStateCommand)x).Items, items)
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnFailureWithoutDispatch_WhenMediaTypeIsUnsupported()
    {
        // Arrange
        await SetupDatabase(
            75,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var library = await IDbContext.PlexLibraries.SingleAsync(CancellationToken);
        var command = new ApplyComparisonStateCommand(
            [CreateItem(886, PlexMediaType.Episode)],
            library.Id,
            PlexMediaType.Episode
        );

        Mock.SetupCommand<Result>(x => x is ApplyOwnedMovieComparisonStateCommand)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);
        Mock.SetupCommand<Result>(x => x is ApplyRemoteMovieComparisonStateCommand)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);
        Mock.SetupCommand<Result>(x => x is ApplyOwnedTvShowComparisonStateCommand)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);
        Mock.SetupCommand<Result>(x => x is ApplyRemoteTvShowComparisonStateCommand)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    private async Task SetOwnedOverrideAsync(int plexServerId, bool ownedOverride)
    {
        await IDbContext
            .PlexServers.Where(x => x.Id == plexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, ownedOverride), CancellationToken);
    }

    private static PlexMediaSlimDTO CreateItem(int id, PlexMediaType mediaType) =>
        new()
        {
            Id = id,
            PlexApiRatingKey = id,
            PlexApiMetaDataKey = id,
            Title = $"Media {id}",
            SearchTitle = $"media {id}",
            SortIndex = id,
            Year = 2026,
            Duration = 120,
            MediaSize = 1024,
            ChildCount = 0,
            GrandChildCount = 0,
            AddedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PlexLibraryId = 0,
            PlexServerId = 0,
            Type = mediaType,
            HasThumb = false,
            Qualities = [],
        };
}
