using Flurl;
using Reaparr.Environment;

// ReSharper disable InconsistentNaming
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace Reaparr.PublicAPI;

public record SearchMovieCommand : ICommand<Result<TorznabMediaSearchResponseDTO>>
{
    public required string Query { get; init; }

    public required int Limit { get; init; }

    public required int Offset { get; init; }

    public string? IMDB_ID { get; init; }

    public required int TMDB_ID { get; init; }
}

public class SearchMovieCommandValidator : AbstractValidator<SearchMovieCommand>
{
    public SearchMovieCommandValidator()
    {
        // Basic argument validation
        RuleFor(x => x.Limit).GreaterThan(0).LessThanOrEqualTo(500);

        RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);

        RuleFor(x => x.TMDB_ID).GreaterThanOrEqualTo(0);
    }
}

public class SearchMovieCommandHandler : ICommandHandler<SearchMovieCommand, Result<TorznabMediaSearchResponseDTO>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IAppRuntimeInfo _appRuntimeInfo;
    private readonly INetworkSettings _networkSettings;
    private readonly IIntegrationsSettings _integrationsSettings;

    public SearchMovieCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IAppRuntimeInfo appRuntimeInfo,
        INetworkSettings networkSettings,
        IIntegrationsSettings integrationsSettings
    )
    {
        _log = log.ForContext<SearchMovieCommandHandler>();
        _dbContext = dbContext;
        _appRuntimeInfo = appRuntimeInfo;
        _networkSettings = networkSettings;
        _integrationsSettings = integrationsSettings;
    }

    public async Task<Result<TorznabMediaSearchResponseDTO>> ExecuteAsync(
        SearchMovieCommand command,
        CancellationToken cancellationToken
    )
    {
        var movies = await LoadMoviesAsync(command, cancellationToken);

        var items = new List<TorznabItem>();
        foreach (var movie in movies)
        {
            items.AddRange(MapMovieToItems(movie));
        }

        return Result.Ok(
            new TorznabMediaSearchResponseDTO
            {
                Channel = new TorznabChannel
                {
                    Title = "Reaparr Indexer",
                    Description = $"Movie Search results for {command.Query}",
                    Language = "en-us",
                    Category = "search",
                    Items = items,
                },
            }
        );
    }

    private async Task<List<PlexMovie>> LoadMoviesAsync(SearchMovieCommand command, CancellationToken cancellationToken)
    {
        var onlineServerIds = await _dbContext.GetOnlineServerIds();
        if (!onlineServerIds.Any())
        {
            _log.Here().Warning("No online Plex servers found, returning empty search results.");
            return [];
        }

        // Base query with required navigation properties for mapping
        var baseQuery = _dbContext
            .PlexMovies.Include(x => x.MediaDataList)
            .Where(x => onlineServerIds.Contains(x.PlexServerId))
            .AsQueryable();

        // If no specific query or external IDs are provided, return a paged list
        var noQueryProvided = string.IsNullOrWhiteSpace(command.Query);
        var noExternalIdsProvided = string.IsNullOrWhiteSpace(command.IMDB_ID) && command.TMDB_ID <= 0;
        if (noQueryProvided && noExternalIdsProvided)
        {
            return await baseQuery
                .OrderBy(m => m.Id) // deterministic paging
                .Skip(command.Offset)
                .Take(command.Limit)
                .ToListAsync(cancellationToken);
        }

        if (!noQueryProvided)
        {
            var searchTitle = command.Query.ToSearchTitle();
            if (string.IsNullOrWhiteSpace(searchTitle))
                return [];

            var likeQuery = $"{searchTitle}%";
            baseQuery = baseQuery.Where(m => EF.Functions.Like(m.SearchTitle, likeQuery));
        }

        // Otherwise apply filters only for the provided external IDs
        if (!string.IsNullOrWhiteSpace(command.IMDB_ID))
            baseQuery = baseQuery.Where(m => m.Guid_IMDB == "tt" + command.IMDB_ID);

        if (command.TMDB_ID > 0)
            baseQuery = baseQuery.Where(m => m.Guid_TMDB == command.TMDB_ID);

        return await baseQuery
            .OrderBy(m => m.Id)
            .Skip(command.Offset)
            .Take(command.Limit)
            .ToListAsync(cancellationToken);
    }

    private IEnumerable<TorznabItem> MapMovieToItems(PlexMovie movie)
    {
        foreach (var mediaData in movie.MediaDataList.OrderBy(md => md.PlexApiPartId))
        {
            var torrentMetadata = new TorrentMetadataDTO
            {
                Type = PlexMediaType.Movie,
                MediaId = movie.Id,
                DataId = mediaData.Id,
                PartId = mediaData.Id, // TODO: Media and Parts are merged in the same DB table, PartId can be removed
                PlexApiPartId = mediaData.PlexApiPartId,
                Quality = mediaData.Quality,
                LibraryId = mediaData.PlexLibraryId,
                ServerId = mediaData.PlexServerId,
            };

            // FORCE this to be a string, and not an implicit URL type by Flurl
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            // The apikey is carried on the link because Sonarr/Radarr fetch release URLs
            // through their indexer HTTP path, which holds no download client session.
            string torrentDownloadUrl = _networkSettings
                .Url.AppendPathSegment(PublicApiRoutes.DownloadTorrent)
                .SetQueryParams(torrentMetadata.Values)
                .SetQueryParam("apikey", _integrationsSettings.ReaparrApiKey);

            _log.Here()
                .Debug(
                    "Generated torrent URL for PlexMovieMediaDataId {PlexMovieMediaDataId}: {Url}",
                    mediaData.Id,
                    torrentDownloadUrl
                );

            var item = new TorznabItem
            {
                Title = mediaData.GetFileName,
                PubDate = movie.AddedAt.ToString("R"),
                Guid = new TorznabGuid { Value = torrentDownloadUrl },
                Link = torrentDownloadUrl,
                Size = mediaData.Size,
                Enclosure = new TorznabEnclosure
                {
                    Url = torrentDownloadUrl,
                    Length = mediaData.Size,
                    Type = "application/x-bittorrent",
                },
            };

            var count = MemeNumberGenerator.GetRandomMemeNumber().ToString();
            item.Attributes.Add(new TorznabAttr("seeders", count));
            item.Attributes.Add(new TorznabAttr("peers", count));
            item.Attributes.Add(new TorznabAttr("type", "movie"));
            item.Attributes.Add(new TorznabAttr("language", "English"));
            item.Attributes.Add(new TorznabAttr("downloadvolumefactor", "0.0"));

            item.Attributes.Add(new TorznabAttr("category", mediaData.ToTorznabMovieCategory().ToString()));
            item.Attributes.Add(new TorznabAttr("resolution", mediaData.VideoResolution.ToResolutionLabel()));
            item.Attributes.Add(new TorznabAttr("source", mediaData.Source.ToEnumMemberValue()));
            item.Attributes.Add(new TorznabAttr("videoCodec", mediaData.VideoCodec));
            item.Attributes.Add(new TorznabAttr("audioCodec", mediaData.AudioCodec));

            if (_appRuntimeInfo.IsDevelopmentEnvironment)
            {
                item.Attributes.Add(new TorznabAttr("debug-plexServerId", mediaData.PlexServerId.ToString()));
                item.Attributes.Add(new TorznabAttr("debug-plexLibraryId", mediaData.PlexLibraryId.ToString()));
                item.Attributes.Add(new TorznabAttr("debug-plexApiMediaId", mediaData.PlexApiMediaId.ToString()));
                item.Attributes.Add(new TorznabAttr("debug-ratingKey", mediaData.PlexApiRatingKey.ToString()));
            }

            if (movie.Guid_TMDB is not null)
                item.Attributes.Add(new TorznabAttr("tmdbid", movie.Guid_TMDB.Value.ToString()));

            if (!string.IsNullOrEmpty(movie.Guid_IMDB))
                item.Attributes.Add(new TorznabAttr("imdb", movie.Guid_IMDB));

            yield return item;
        }
    }
}
