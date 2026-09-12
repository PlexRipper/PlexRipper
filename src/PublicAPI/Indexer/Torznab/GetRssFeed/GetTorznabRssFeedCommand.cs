using Reaparr.Application.Contracts;

namespace Reaparr.PublicAPI;

public record GetTorznabRssFeedCommand : ICommand<Result<TorznabMediaSearchResponseDTO>>
{
    public required IntegrationIdentity Integration { get; init; }
    public required int[] Categories { get; init; }
    public required bool IncludeMovies { get; init; }
    public required bool IncludeEpisodes { get; init; }
    public required int Limit { get; init; }
    public required int Offset { get; init; }
    public required string TorznabApiKey { get; init; }
    public required string[] Attributes { get; init; }
    public required bool IncludeAllAttributes { get; init; }
}

public class GetTorznabRssFeedCommandValidator : AbstractValidator<GetTorznabRssFeedCommand>
{
    public GetTorznabRssFeedCommandValidator()
    {
        RuleFor(x => x.Integration).NotNull();
        RuleFor(x => x.Categories).NotNull();
        RuleFor(x => x.Limit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Attributes).NotNull();
    }
}

public class GetTorznabRssFeedCommandHandler
    : ICommandHandler<GetTorznabRssFeedCommand, Result<TorznabMediaSearchResponseDTO>>
{
    private static readonly int[] _movieCategories =
    [
        (int)TorznabCategoryId.Movies,
        (int)TorznabCategoryId.Movies_SD,
        (int)TorznabCategoryId.Movies_HD,
        (int)TorznabCategoryId.Movies_UHD,
        (int)TorznabCategoryId.Movies_BluRay,
        (int)TorznabCategoryId.Movies_WEBDL,
    ];

    private static readonly int[] _tvCategories =
    [
        (int)TorznabCategoryId.TV,
        (int)TorznabCategoryId.TV_SD,
        (int)TorznabCategoryId.TV_HD,
        (int)TorznabCategoryId.TV_UHD,
    ];

    private readonly IReaparrDbContext _dbContext;
    private readonly INetworkSettings _networkSettings;

    public GetTorznabRssFeedCommandHandler(IReaparrDbContext dbContext, INetworkSettings networkSettings)
    {
        _dbContext = dbContext;
        _networkSettings = networkSettings;
    }

    public async Task<Result<TorznabMediaSearchResponseDTO>> ExecuteAsync(
        GetTorznabRssFeedCommand command,
        CancellationToken cancellationToken
    )
    {
        IQueryable<TorznabFeedItemProjection>? query = null;
        if (command.IncludeMovies)
            query = CreateMovieQuery(command.Categories);
        if (command.IncludeEpisodes)
        {
            var episodeQuery = CreateEpisodeQuery(command.Categories);
            query = query is null ? episodeQuery : query.Concat(episodeQuery);
        }

        if (query is null)
            return Result.Ok(CreateResponse(command.Offset, 0, []));

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(x => x.AddedAt)
            .ThenByDescending(x => x.PlexServerMachineIdentifier)
            .ThenByDescending(x => x.PlexApiMediaId)
            .ThenByDescending(x => x.PlexApiPartId)
            .Skip(command.Offset)
            .Take(command.Limit)
            .ToListAsync(cancellationToken);
        var requestedAttributes = command.IncludeAllAttributes
            ? null
            : command.Attributes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var items = rows.Select(x =>
                x.ToTorznabItem(command.Integration, command.TorznabApiKey, _networkSettings.Url, requestedAttributes)
            )
            .ToList();
        return Result.Ok(CreateResponse(command.Offset, total, items));
    }

    private IQueryable<TorznabFeedItemProjection> CreateMovieQuery(int[] categories)
    {
        var query = _dbContext.PlexMovieData.Where(x =>
            x.PlexServer!.ServerStatus.Any(status => status.IsSuccessful)
            && x.PlexServer.IsEnabled
            && !x.PlexServer.IsDownloadsPausedByUser
            && x.PlexMovie!.PlexLibrary!.PlexAccountLibraries.Any(libraryAccess =>
                x.PlexMovie.PlexServer!.PlexAccountServers.Any(serverAccess =>
                    serverAccess.PlexAccountId == libraryAccess.PlexAccountId
                )
            )
        );
        query = ApplyMovieCategories(query, categories);
        return query.Select(x => new TorznabFeedItemProjection
        {
            MediaType = PlexMediaType.Movie,
            MediaId = x.PlexMovieId,
            DataId = x.Id,
            PlexServerId = x.PlexServerId,
            PlexServerMachineIdentifier = x.PlexServer!.MachineIdentifier,
            PlexLibraryId = x.PlexLibraryId,
            PlexApiRatingKey = x.PlexApiRatingKey,
            PlexApiMediaId = x.PlexApiMediaId,
            PlexApiPartId = x.PlexApiPartId,
            Title = !string.IsNullOrEmpty(x.GeneratedFilename) ? x.GeneratedFilename : x.OriginalFilename,
            AddedAt = x.PlexMovie!.AddedAt,
            Size = x.Size,
            Quality = x.Quality,
            VideoResolution = x.VideoResolution,
            Source = x.Source,
            VideoCodec = x.VideoCodec,
            AudioCodec = x.AudioCodec,
            SeasonNumber = null,
            EpisodeNumber = null,
            TvdbId = null,
            TmdbId = x.PlexMovie.Guid_TMDB,
            ImdbId = x.PlexMovie.Guid_IMDB,
        });
    }

    private IQueryable<TorznabFeedItemProjection> CreateEpisodeQuery(int[] categories)
    {
        var query = _dbContext.PlexTvShowEpisodeData.Where(x =>
            x.PlexServer!.ServerStatus.Any(status => status.IsSuccessful)
            && x.PlexServer.IsEnabled
            && !x.PlexServer.IsDownloadsPausedByUser
            && x.PlexTvShowEpisode!.TvShow!.PlexLibrary!.PlexAccountLibraries.Any(libraryAccess =>
                x.PlexTvShowEpisode.TvShow.PlexServer!.PlexAccountServers.Any(serverAccess =>
                    serverAccess.PlexAccountId == libraryAccess.PlexAccountId
                )
            )
        );
        query = ApplyEpisodeCategories(query, categories);
        return query.Select(x => new TorznabFeedItemProjection
        {
            MediaType = PlexMediaType.Episode,
            MediaId = x.PlexTvShowEpisodeId,
            DataId = x.Id,
            PlexServerId = x.PlexServerId,
            PlexServerMachineIdentifier = x.PlexServer!.MachineIdentifier,
            PlexLibraryId = x.PlexLibraryId,
            PlexApiRatingKey = x.PlexApiRatingKey,
            PlexApiMediaId = x.PlexApiMediaId,
            PlexApiPartId = x.PlexApiPartId,
            Title = !string.IsNullOrEmpty(x.GeneratedFilename) ? x.GeneratedFilename : x.OriginalFilename,
            AddedAt = x.PlexTvShowEpisode!.AddedAt,
            Size = x.Size,
            Quality = x.Quality,
            VideoResolution = x.VideoResolution,
            Source = x.Source,
            VideoCodec = x.VideoCodec,
            AudioCodec = x.AudioCodec,
            SeasonNumber = x.PlexTvShowEpisode.TvShowSeason!.SeasonNumber,
            EpisodeNumber = x.PlexTvShowEpisode.EpisodeNumber,
            TvdbId = x.PlexTvShowEpisode.TvShow!.Guid_TVDB,
            TmdbId = x.PlexTvShowEpisode.TvShow.Guid_TMDB,
            ImdbId = x.PlexTvShowEpisode.TvShow.Guid_IMDB,
        });
    }

    private static TorznabMediaSearchResponseDTO CreateResponse(int offset, int total, List<TorznabItem> items) =>
        new()
        {
            Channel = new TorznabChannel
            {
                Title = "Reaparr Indexer",
                Description = "Reaparr RSS feed",
                Language = "en-us",
                Category = "search",
                Items = items,
                Response = new TorznabResponseMetadata { Offset = offset, Total = total },
            },
        };

    private static IQueryable<PlexMovieMediaData> ApplyMovieCategories(
        IQueryable<PlexMovieMediaData> query,
        int[] categories
    )
    {
        if (categories.Length == 0 || categories.Contains((int)TorznabCategoryId.Movies))
            return query;

        var known = categories.Intersect(_movieCategories).ToArray();
        if (known.Length == 0)
            return query.Where(_ => false);

        return query.Where(x =>
            (
                known.AsEnumerable().Contains((int)TorznabCategoryId.Movies_SD)
                && (
                    x.Source == ReleaseSource.DVD
                    || x.VideoResolution == VideoQuality.SD
                    || x.VideoResolution == VideoQuality.DVD
                )
            )
            || (
                known.AsEnumerable().Contains((int)TorznabCategoryId.Movies_UHD)
                && x.Source != ReleaseSource.DVD
                && (x.VideoResolution == VideoQuality.UHD_4K || x.VideoResolution == VideoQuality.UHD_8K)
            )
            || (
                known.AsEnumerable().Contains((int)TorznabCategoryId.Movies_HD)
                && x.Source != ReleaseSource.DVD
                && x.VideoResolution != VideoQuality.SD
                && x.VideoResolution != VideoQuality.DVD
                && x.VideoResolution != VideoQuality.UHD_4K
                && x.VideoResolution != VideoQuality.UHD_8K
            )
            || (
                known.AsEnumerable().Contains((int)TorznabCategoryId.Movies_BluRay)
                && (x.Source == ReleaseSource.BluRay || x.Source == ReleaseSource.BluRayRemux)
            )
            || (
                known.AsEnumerable().Contains((int)TorznabCategoryId.Movies_WEBDL)
                && (x.Source == ReleaseSource.WebDl || x.Source == ReleaseSource.WebRip)
            )
        );
    }

    private static IQueryable<PlexTvShowEpisodeMediaData> ApplyEpisodeCategories(
        IQueryable<PlexTvShowEpisodeMediaData> query,
        int[] categories
    )
    {
        if (categories.Length == 0 || categories.Contains((int)TorznabCategoryId.TV))
            return query;

        var known = categories.Intersect(_tvCategories).ToArray();
        if (known.Length == 0)
            return query.Where(_ => false);

        return query.Where(x =>
            (
                known.AsEnumerable().Contains((int)TorznabCategoryId.TV_SD)
                && (
                    x.Source == ReleaseSource.DVD
                    || x.VideoResolution == VideoQuality.SD
                    || x.VideoResolution == VideoQuality.DVD
                )
            )
            || (
                known.AsEnumerable().Contains((int)TorznabCategoryId.TV_UHD)
                && x.Source != ReleaseSource.DVD
                && (x.VideoResolution == VideoQuality.UHD_4K || x.VideoResolution == VideoQuality.UHD_8K)
            )
            || (
                known.AsEnumerable().Contains((int)TorznabCategoryId.TV_HD)
                && x.Source != ReleaseSource.DVD
                && x.VideoResolution != VideoQuality.SD
                && x.VideoResolution != VideoQuality.DVD
                && x.VideoResolution != VideoQuality.UHD_4K
                && x.VideoResolution != VideoQuality.UHD_8K
            )
        );
    }
}
