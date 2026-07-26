namespace Reaparr.Application.UnitTests;

public class GetMediaComparisonDetailsEndpointUnitTests
    : BaseEndpointUnitTest<GetMediaComparisonDetailsEndpoint, GetMediaComparisonDetailsEndpointRequest, ResultDTO<PlexMediaComparisonDetailsDTO>>
{
    [Test]
    public async Task ShouldReturnSeasonRowsWithSeasonPlexMediaIds_WhenRemoteTvShowHasMissingEpisodes()
    {
        // Arrange
        await SetupDatabase(
            53445,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 2;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 22, 10, 5, 14, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, new DateTime(2026, 7, 22, 9, 45, 2, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        ownedLibrary = await GetLibraryAsync(ownedLibrary.Id);

        var remoteTvShow = await GetLibraryTvShowAsync(remoteLibrary.Id);
        var remoteSeasons = await dbContext.PlexTvShowSeason
            .Where(x => x.TvShowId == remoteTvShow.Id)
            .OrderBy(x => x.SeasonNumber)
            .ToListAsync(CancellationToken);
        var remoteEpisodes = await dbContext.PlexTvShowEpisodes
            .Where(x => x.TvShowId == remoteTvShow.Id)
            .OrderBy(x => x.TvShowSeasonId)
            .ThenBy(x => x.EpisodeNumber)
            .ToListAsync(CancellationToken);
        remoteSeasons.Count.ShouldBe(2);
        remoteEpisodes.Count.ShouldBe(4);

        await AddCurrentScopeAsync(remoteLibrary, ownedLibrary);
        await dbContext.SaveChangesNewAsync(CancellationToken);

        var request = new GetMediaComparisonDetailsEndpointRequest(remoteTvShow.Id, PlexMediaType.TvShow);

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        result.Value.ShouldNotBeNull();
        result.Value.Rows.Count.ShouldBe(2);
        foreach (var seasonRow in result.Value.Rows.OrderBy(x => x.Title))
        {
            var seasonNumber = int.Parse(seasonRow.Title.Replace("Season ", string.Empty));
            var expectedSeason = remoteSeasons.Single(x => x.SeasonNumber == seasonNumber);
            var expectedEpisodeIds = remoteEpisodes
                .Where(x => x.TvShowSeasonId == expectedSeason.Id)
                .Select(x => x.Id)
                .OrderBy(x => x)
                .ToList();

            seasonRow.PlexMediaId.ShouldBe(expectedSeason.Id);
            seasonRow.PlexMediaId.ShouldBeGreaterThan(0);
            seasonRow.Type.ShouldBe(PlexMediaType.Season);
            seasonRow.State.ShouldBe(PlexMediaComparisonState.Partial);
            seasonRow.Children.Select(x => x.PlexMediaId).OrderBy(x => x).ShouldBe(expectedEpisodeIds);
            seasonRow.Children.ShouldAllBe(x => x.Type == PlexMediaType.Episode);
            seasonRow.Children.ShouldAllBe(x => x.State == PlexMediaComparisonState.Missing);
        }
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
        await dbContext.SaveChangesNewAsync(CancellationToken);
    }
}