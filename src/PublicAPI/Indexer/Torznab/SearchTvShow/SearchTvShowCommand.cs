using System.Diagnostics.CodeAnalysis;
using Flurl;
using Reaparr.Application.Contracts;

// ReSharper disable InconsistentNaming

namespace Reaparr.PublicAPI;

public record SearchTvShowCommand : ICommand<Result<TorznabMediaSearchResponseDTO>>
{
    [SetsRequiredMembers]
    public SearchTvShowCommand()
    {
        Query = string.Empty;
        IMDB_ID = string.Empty;
        Integration = new IntegrationIdentity(IntegrationType.Sonarr, Guid.Empty);
    }

    public required string Query { get; init; }

    public required int Season { get; init; }

    public required int Episode { get; init; }

    public required int Limit { get; init; }

    public required int Offset { get; init; }

    public required int TVDB_ID { get; init; }

    public required int TMDB_ID { get; init; }

    public required string IMDB_ID { get; init; }

    public required IntegrationIdentity Integration { get; init; }

    public string TorznabApiKey { get; init; } = string.Empty;
}

public class SearchTvShowCommandValidator : AbstractValidator<SearchTvShowCommand>
{
    public SearchTvShowCommandValidator()
    {
        // Basic argument validation
        RuleFor(x => x.Limit).GreaterThan(0).LessThanOrEqualTo(500);

        RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);

        RuleFor(x => x.Season).GreaterThanOrEqualTo(0);

        RuleFor(x => x.Episode).GreaterThanOrEqualTo(0);

        RuleFor(x => x.TMDB_ID).GreaterThanOrEqualTo(0);

        RuleFor(x => x.TVDB_ID).GreaterThanOrEqualTo(0);

        RuleFor(x => x.IMDB_ID).NotNull();

        // When targeting a season or episode, a season and an external ID must identify the show.
        When(
            x => x.Season > 0 || x.Episode > 0,
            () =>
            {
                RuleFor(x => x.Season).GreaterThan(0);

                RuleFor(x => x)
                    .Must(HasAnyExternalId)
                    .WithMessage(
                        "Provide at least one of IMDB_ID, TMDB_ID, or TVDB_ID when Season/Episode are specified."
                    );
            }
        );
    }

    private static bool HasAnyExternalId(SearchTvShowCommand cmd) =>
        !string.IsNullOrWhiteSpace(cmd.IMDB_ID) || cmd.TMDB_ID > 0 || cmd.TVDB_ID > 0;
}

public class SearchTvShowCommandHandler : ICommandHandler<SearchTvShowCommand, Result<TorznabMediaSearchResponseDTO>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly INetworkSettings _networkSettings;

    public SearchTvShowCommandHandler(ILogger log, IReaparrDbContext dbContext, INetworkSettings networkSettings)
    {
        _log = log.ForContext<SearchTvShowCommandHandler>();
        _dbContext = dbContext;
        _networkSettings = networkSettings;
    }

    public async Task<Result<TorznabMediaSearchResponseDTO>> ExecuteAsync(
        SearchTvShowCommand command,
        CancellationToken cancellationToken
    )
    {
        var episodes = await LoadEpisodesAsync(command, cancellationToken);

        var items = new List<TorznabItem>();
        foreach (var episode in episodes)
        {
            items.AddRange(MapEpisodeToItems(episode, command));
        }

        return Result.Ok(
            new TorznabMediaSearchResponseDTO
            {
                Channel = new TorznabChannel
                {
                    Title = "Reaparr Indexer",
                    Description = $"TV Search results for {command.Query}",
                    Language = "en-us",
                    Category = "search",
                    Items = items,
                },
            }
        );
    }

    private async Task<List<PlexTvShowEpisode>> LoadEpisodesAsync(
        SearchTvShowCommand command,
        CancellationToken cancellationToken
    )
    {
        var onlineServerIds = await _dbContext.GetDownloadableServerIds();
        if (!onlineServerIds.Any())
        {
            _log.Here()
                .Warning(
                    "No online Plex servers with downloads enabled were found, returning empty search results."
                );
            return [];
        }

        // Base query with required navigation properties for mapping
        var baseQuery = _dbContext
            .PlexTvShowEpisodes.AsNoTracking()
            .Include(x => x.TvShowSeason)
            .Include(x => x.TvShow)
            .Include(e => e.MediaDataList)
            .Where(x => onlineServerIds.Contains(x.PlexServerId))
            .AsQueryable();

        var hasSeason = command.Season > 0;
        var hasEpisode = command.Episode > 0;
        var hasSeasonOrEpisode = hasSeason || hasEpisode;

        if (!string.IsNullOrWhiteSpace(command.Query))
        {
            var searchTitle = command.Query.ToSearchTitle();
            if (string.IsNullOrWhiteSpace(searchTitle))
                return [];

            var likeQuery = $"{searchTitle}%";
            baseQuery = baseQuery.Where(e => EF.Functions.Like(e.TvShow!.SearchTitle, likeQuery));
        }

        // Apply external ID predicates only when provided.
        if (!string.IsNullOrWhiteSpace(command.IMDB_ID))
            baseQuery = baseQuery.Where(e => e.TvShow!.Guid_IMDB == "tt" + command.IMDB_ID);

        if (command.TMDB_ID > 0)
            baseQuery = baseQuery.Where(e => e.TvShow!.Guid_TMDB == command.TMDB_ID);

        if (command.TVDB_ID > 0)
            baseQuery = baseQuery.Where(e => e.TvShow!.Guid_TVDB == command.TVDB_ID);

        if (hasSeason)
            baseQuery = baseQuery.Where(e => e.TvShowSeason!.SeasonNumber == command.Season);

        if (hasEpisode)
            baseQuery = baseQuery.Where(e => e.EpisodeNumber == command.Episode);

        if (!hasSeasonOrEpisode)
        {
            return await baseQuery
                .OrderBy(e => e.Id) // deterministic paging
                .Skip(command.Offset)
                .Take(command.Limit)
                .ToListAsync(cancellationToken);
        }

        return await baseQuery.OrderBy(e => e.Id).ToListAsync(cancellationToken);
    }

    private IEnumerable<TorznabItem> MapEpisodeToItems(PlexTvShowEpisode episode, SearchTvShowCommand command)
    {
        var tvShow = episode.TvShow;
        var season = episode.TvShowSeason;

        if (tvShow is null)
        {
            _log.Here().Warning("TvShow is null for episode {EpisodeId}", episode.Id);
            yield break;
        }

        if (season is null)
        {
            _log.Here().Warning("Season is null for episode {EpisodeId}", episode.Id);
            yield break;
        }

        foreach (var mediaData in episode.MediaDataList)
        {
            var torrentMetadata = new TorrentMetadataDTO
            {
                Type = PlexMediaType.Episode,
                MediaId = episode.Id,
                DataId = mediaData.Id,
                PartId = mediaData.Id, // TODO: Media and Parts are merged in the same DB table, PartId can be removed
                PlexApiPartId = mediaData.PlexApiPartId,
                Quality = mediaData.Quality,
                LibraryId = mediaData.PlexLibraryId,
                ServerId = mediaData.PlexServerId,
            };

            // FORCE this to be a string, and not an implicit URL type by Flurl
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            string torrentDownloadUrl = _networkSettings
                .Url.AppendPathSegment(
                    PublicApiRoutes.DownloadTorrent.Replace("{integrationId:guid}", command.Integration.Id.ToString())
                )
                .SetQueryParams(torrentMetadata.Values)
                .SetQueryParam(IntegrationDefinitions.INDEXER_API_KEY, command.TorznabApiKey, isEncoded: false);

            var item = new TorznabItem
            {
                Title = mediaData.GetFileName,
                PubDate = episode.AddedAt.ToString("R"),
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
            item.Attributes.Add(new TorznabAttr("season", season.SeasonNumber.ToString()));
            item.Attributes.Add(new TorznabAttr("episode", episode.EpisodeNumber.ToString()));
            item.Attributes.Add(new TorznabAttr("type", "series"));
            item.Attributes.Add(new TorznabAttr("language", "English"));
            item.Attributes.Add(new TorznabAttr("downloadvolumefactor", "0.0"));

            if (tvShow.Guid_TVDB is not null)
                item.Attributes.Add(new TorznabAttr("tvdbid", tvShow.Guid_TVDB.Value.ToString()));

            if (tvShow.Guid_TMDB is not null)
                item.Attributes.Add(new TorznabAttr("tmdbid", tvShow.Guid_TMDB.Value.ToString()));

            if (!string.IsNullOrEmpty(tvShow.Guid_IMDB))
                item.Attributes.Add(new TorznabAttr("imdb", tvShow.Guid_IMDB));

            yield return item;
        }
    }
}
