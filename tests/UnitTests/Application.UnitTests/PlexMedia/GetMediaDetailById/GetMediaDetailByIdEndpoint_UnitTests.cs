using Reaparr.Application.Contracts.Validators;

namespace Reaparr.Application.UnitTests;

public class GetMediaDetailByIdEndpointUnitTests : BaseEndpointUnitTest<GetMediaDetailByIdEndpoint, GetMediaDetailByIdEndpointRequest, ResultDTO<PlexMediaDTO>>
{
    private PlexMediaDTOValidator PlexMediaDtoValidator => new();

    [Test]
    public async Task ShouldHavePlexMediaData_WhenValidMediaIdAndPlexMediaTypeMovieIsRequested()
    {
        // Arrange
        var movieCount = 10;
        await SetupDatabase(
            45588,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.MovieCount = movieCount;
            }
        );

        var testMovie = IDbContext.PlexMovies.FirstOrDefault(x => x.HasThumb);
        testMovie.ShouldNotBeNull();

        var request = new GetMediaDetailByIdEndpointRequest(testMovie.Id, PlexMediaType.Movie);

        Mock.SetupCommand<Result>(x => x is ApplyComparisonStateCommand).ReturnsAsync(Result.Ok());

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();

        var validationResult = await PlexMediaDtoValidator.ValidateAsync(result.Value, CancellationToken);
        validationResult.Errors.ShouldBeEmpty();
        result.Value.Children.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldHavePlexMediaData_WhenValidMediaIdAndPlexMediaTypeTvShowIsRequested()
    {
        // Arrange
        await SetupDatabase(
            53442,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 10;
                config.TvShowSeasonCount = 3;
                config.TvShowEpisodeCount = 5;
            }
        );

        var testTvShow = IDbContext.PlexTvShows.FirstOrDefault(x => x.HasThumb);
        testTvShow.ShouldNotBeNull();

        var request = new GetMediaDetailByIdEndpointRequest(testTvShow.Id, PlexMediaType.TvShow);

        Mock.SetupCommand<Result>(x => x is ApplyComparisonStateCommand).ReturnsAsync(Result.Ok());

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();

        var validationResult = await PlexMediaDtoValidator.ValidateAsync(result.Value, CancellationToken);
        validationResult.Errors.ShouldBeEmpty();
        result.Value.Children.ShouldNotBeEmpty();
        foreach (var season in result.Value.Children)
        {
            var validationSeasonResult = await PlexMediaDtoValidator.ValidateAsync(season, CancellationToken);
            validationSeasonResult.Errors.ShouldBeEmpty();
            season.Children.ShouldNotBeEmpty();
            foreach (var episode in season.Children)
            {
                var validationEpisode = await PlexMediaDtoValidator.ValidateAsync(episode, CancellationToken);
                validationEpisode.Errors.ShouldBeEmpty();
                episode.Children.ShouldBeEmpty();
            }
        }
    }

    [Test]
    public async Task ShouldProjectEpisodeComparisonStates_WhenRemoteTvShowDetailHasCurrentOwnedComparisonScope()
    {
        // Arrange
        await SetupDatabase(
            53443,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 3;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        libraries.Count.ShouldBe(2);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 21, 17, 24, 15, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, new DateTime(2026, 7, 21, 14, 8, 33, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        ownedLibrary = await GetLibraryAsync(ownedLibrary.Id);

        var remoteTvShow = await GetLibraryTvShowAsync(remoteLibrary.Id);
        var ownedTvShow = await GetLibraryTvShowAsync(ownedLibrary.Id);
        var ownedEpisodes = await dbContext.PlexTvShowEpisodes
            .Where(x => x.TvShowId == ownedTvShow.Id)
            .OrderBy(x => x.Id)
            .Take(2)
            .ToListAsync(CancellationToken);
        var remoteEpisodes = await dbContext.PlexTvShowEpisodes
            .Where(x => x.TvShowId == remoteTvShow.Id)
            .OrderBy(x => x.Id)
            .Take(3)
            .ToListAsync(CancellationToken);
        remoteEpisodes.Count.ShouldBe(3);
        ownedEpisodes.Count.ShouldBe(2);

        await AddCurrentScopeAsync(remoteLibrary, ownedLibrary);
        dbContext.PlexTvShowComparisons.Add(CreateTvShowComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteTvShow.Id,
            ownedTvShow.Id,
            PlexMediaComparisonHitState.HigherQuality
        ));
        dbContext.PlexEpisodeComparisons.Add(CreateEpisodeComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteEpisodes[0].Id,
            ownedEpisodes[0].Id,
            PlexMediaComparisonHitState.Matched
        ));
        dbContext.PlexEpisodeComparisons.Add(CreateEpisodeComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteEpisodes[1].Id,
            ownedEpisodes[1].Id,
            PlexMediaComparisonHitState.HigherQuality
        ));
        await dbContext.SaveChangesAsync(CancellationToken);

        var request = new GetMediaDetailByIdEndpointRequest(remoteTvShow.Id, PlexMediaType.TvShow);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ApplyComparisonStateCommand>(), It.IsAny<CancellationToken>()))
            .Callback<ICommand<Result>, CancellationToken>((cmd, _) =>
            {
                var items = ((ApplyComparisonStateCommand)cmd).Items;
                items[0].ComparisonId = PlexMediaComparisonState.HigherQuality.ToComparisonId();
                items[1].ComparisonId = PlexMediaComparisonState.Owned.ToComparisonId();
                items[2].ComparisonId = PlexMediaComparisonState.HigherQuality.ToComparisonId();
                items[3].ComparisonId = PlexMediaComparisonState.Missing.ToComparisonId();
            })
            .ReturnsAsync(Result.Ok());

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();
        result.Value.ComparisonId.ShouldBe(PlexMediaComparisonState.HigherQuality.ToComparisonId());
        var episodes = result.Value.Children.SelectMany(x => x.Children).OrderBy(x => x.Id).ToList();
        episodes.Count.ShouldBe(3);
        episodes[0].ComparisonId.ShouldBe(PlexMediaComparisonState.Owned.ToComparisonId());
        episodes[1].ComparisonId.ShouldBe(PlexMediaComparisonState.HigherQuality.ToComparisonId());
        episodes[2].ComparisonId.ShouldBe(PlexMediaComparisonState.Missing.ToComparisonId());
    }

    [Test]
    public async Task ShouldProjectShowHigherQualityAndEpisodesOwned_WhenRemoteTvShowDetailHasUpgradeAvailableButAllEpisodesMatched()
    {
        // Arrange
        await SetupDatabase(
            53444,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 3;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        libraries.Count.ShouldBe(2);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 21, 17, 24, 15, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, new DateTime(2026, 7, 21, 14, 8, 33, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        ownedLibrary = await GetLibraryAsync(ownedLibrary.Id);

        var remoteTvShow = await GetLibraryTvShowAsync(remoteLibrary.Id);
        var ownedTvShow = await GetLibraryTvShowAsync(ownedLibrary.Id);
        var remoteEpisodes = await dbContext.PlexTvShowEpisodes
            .Where(x => x.TvShowId == remoteTvShow.Id)
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);
        var ownedEpisodes = await dbContext.PlexTvShowEpisodes
            .Where(x => x.TvShowId == ownedTvShow.Id)
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);
        remoteEpisodes.Count.ShouldBe(3);
        ownedEpisodes.Count.ShouldBe(3);
        await dbContext.PlexTvShowSeason
            .Where(x => x.TvShowId == remoteTvShow.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.ChildCount, remoteEpisodes.Count), CancellationToken);

        await AddCurrentScopeAsync(remoteLibrary, ownedLibrary);
        dbContext.PlexTvShowComparisons.Add(CreateTvShowComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteTvShow.Id,
            ownedTvShow.Id,
            PlexMediaComparisonHitState.HigherQuality
        ));
        for (var i = 0; i < remoteEpisodes.Count; i++)
        {
            dbContext.PlexEpisodeComparisons.Add(CreateEpisodeComparison(
                remoteLibrary.Id,
                ownedLibrary.Id,
                remoteEpisodes[i].Id,
                ownedEpisodes[i].Id,
                PlexMediaComparisonHitState.Matched
            ));
        }

        await dbContext.SaveChangesAsync(CancellationToken);

        var request = new GetMediaDetailByIdEndpointRequest(remoteTvShow.Id, PlexMediaType.TvShow);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ApplyComparisonStateCommand>(), It.IsAny<CancellationToken>()))
            .Callback<ICommand<Result>, CancellationToken>((cmd, _) =>
            {
                var items = ((ApplyComparisonStateCommand)cmd).Items;
                items[0].ComparisonId = PlexMediaComparisonState.HigherQuality.ToComparisonId();
                for (var i = 1; i < items.Count; i++)
                    items[i].ComparisonId = PlexMediaComparisonState.Owned.ToComparisonId();
            })
            .ReturnsAsync(Result.Ok());

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();
        result.Value.ComparisonId.ShouldBe(PlexMediaComparisonState.HigherQuality.ToComparisonId());
        var episodes = result.Value.Children.SelectMany(x => x.Children).OrderBy(x => x.Id).ToList();
        episodes.Count.ShouldBe(3);
        episodes.ShouldAllBe(x => x.ComparisonId == PlexMediaComparisonState.Owned.ToComparisonId());
    }

    private async Task SetOwnedOverrideAsync(int plexServerId, bool ownedOverride)
    {
        await IDbContext.PlexServers
            .Where(x => x.Id == plexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, ownedOverride), CancellationToken);
    }

    private async Task SetLibraryUpdatedAtAsync(int plexLibraryId, DateTime updatedAt)
    {
        await IDbContext.PlexLibraries
            .Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.UpdatedAt, updatedAt), CancellationToken);
    }

    private async Task<PlexLibrary> GetLibraryAsync(int plexLibraryId) =>
        await IDbContext.PlexLibraries
            .Where(x => x.Id == plexLibraryId)
            .SingleAsync(CancellationToken);

    private async Task<PlexTvShow> GetLibraryTvShowAsync(int plexLibraryId) =>
        await IDbContext.PlexTvShows
            .Where(x => x.PlexLibraryId == plexLibraryId)
            .SingleAsync(CancellationToken);

    private async Task AddCurrentScopeAsync(PlexLibrary remoteLibrary, PlexLibrary ownedLibrary)
    {
        var remoteUpdatedAt = await IDbContext.PlexLibraries
            .Where(x => x.Id == remoteLibrary.Id)
            .Select(x => x.UpdatedAt)
            .SingleAsync(CancellationToken);
        var ownedUpdatedAt = await IDbContext.PlexLibraries
            .Where(x => x.Id == ownedLibrary.Id)
            .Select(x => x.UpdatedAt)
            .SingleAsync(CancellationToken);

        var dbContext = IDbContext;
        dbContext.PlexComparisonScopes.Add(new PlexComparisonState
        {
            Id = 0,
            RemotePlexLibraryId = remoteLibrary.Id,
            OwnedPlexLibraryId = ownedLibrary.Id,
            MediaType = PlexMediaType.TvShow,
            CompletedAt = DateTime.UtcNow,
            RemoteLibraryUpdatedAt = remoteUpdatedAt,
            OwnedLibraryUpdatedAt = ownedUpdatedAt,
        });
        await dbContext.SaveChangesAsync(CancellationToken);
    }

    private static PlexTvShowComparison CreateTvShowComparison(
        int remotePlexLibraryId,
        int ownedPlexLibraryId,
        int remotePlexMediaId,
        int ownedPlexMediaId,
        PlexMediaComparisonHitState hitState) =>
        new()
        {
            Id = 0,
            RemotePlexLibraryId = remotePlexLibraryId,
            OwnedPlexLibraryId = ownedPlexLibraryId,
            RemotePlexMediaId = remotePlexMediaId,
            OwnedPlexMediaId = ownedPlexMediaId,
            HitState = hitState,
            RemoteQuality = VideoQuality.FullHD,
            OwnedQuality = hitState == PlexMediaComparisonHitState.HigherQuality ? VideoQuality.HD : VideoQuality.FullHD,
            MatchType = PlexMediaComparisonMatchType.TmdbGuid,
            ComparedAt = DateTime.UtcNow,
        };

    private static PlexEpisodeComparison CreateEpisodeComparison(
        int remotePlexLibraryId,
        int ownedPlexLibraryId,
        int remotePlexMediaId,
        int ownedPlexMediaId,
        PlexMediaComparisonHitState hitState) =>
        new()
        {
            Id = 0,
            RemotePlexLibraryId = remotePlexLibraryId,
            OwnedPlexLibraryId = ownedPlexLibraryId,
            RemotePlexMediaId = remotePlexMediaId,
            OwnedPlexMediaId = ownedPlexMediaId,
            HitState = hitState,
            RemoteQuality = VideoQuality.FullHD,
            OwnedQuality = hitState == PlexMediaComparisonHitState.HigherQuality ? VideoQuality.HD : VideoQuality.FullHD,
            MatchType = PlexMediaComparisonMatchType.TmdbGuid,
            ComparedAt = DateTime.UtcNow,
        };
}

