using System.Diagnostics.CodeAnalysis;
using Reaparr.Application.Contracts;
using Reaparr.Environment;

// ReSharper disable InconsistentNaming
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace Reaparr.PublicAPI;

public record SearchMovieCommand : ICommand<Result<TorznabMediaSearchResponseDTO>>
{
    [SetsRequiredMembers]
    public SearchMovieCommand()
    {
        Query = string.Empty;
        Integration = new IntegrationIdentity(IntegrationType.Radarr, Guid.Empty);
    }

    public required string Query { get; init; }

    public required int Limit { get; init; }

    public required int Offset { get; init; }

    public string? IMDB_ID { get; init; }

    public required int TMDB_ID { get; init; }

    public required IntegrationIdentity Integration { get; init; }

    public string TorznabApiKey { get; init; } = string.Empty;
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

    public SearchMovieCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IAppRuntimeInfo appRuntimeInfo,
        INetworkSettings networkSettings
    )
    {
        _log = log.ForContext<SearchMovieCommandHandler>();
        _dbContext = dbContext;
        _appRuntimeInfo = appRuntimeInfo;
        _networkSettings = networkSettings;
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
            items.AddRange(MapMovieToItems(movie, command));
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
        var onlineServerIds = await _dbContext.GetDownloadableServerIds();
        if (!onlineServerIds.Any())
        {
            _log.Here()
                .Warning("No online Plex servers with downloads enabled were found, returning empty search results");
            return [];
        }

        // Base query with required navigation properties for mapping
        var baseQuery = _dbContext
            .PlexMovies.Include(x => x.PlexServer)
            .Include(x => x.MediaDataList)
            .Where(x => onlineServerIds.Contains(x.PlexServerId))
            .WhereHasPlexAccountAccess()
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

    private IEnumerable<TorznabItem> MapMovieToItems(PlexMovie movie, SearchMovieCommand command)
    {
        foreach (var mediaData in movie.MediaDataList.OrderBy(x => x.PlexApiPartId))
        {
            yield return new TorznabFeedItemProjection
            {
                MediaType = PlexMediaType.Movie,
                MediaId = movie.Id,
                DataId = mediaData.Id,
                PlexServerId = mediaData.PlexServerId,
                PlexServerMachineIdentifier = movie.PlexServer!.MachineIdentifier,
                PlexLibraryId = mediaData.PlexLibraryId,
                PlexApiRatingKey = mediaData.PlexApiRatingKey,
                PlexApiMediaId = mediaData.PlexApiMediaId,
                PlexApiPartId = mediaData.PlexApiPartId,
                Title = mediaData.GetFileName,
                AddedAt = movie.AddedAt,
                Size = mediaData.Size,
                Quality = mediaData.Quality,
                VideoResolution = mediaData.VideoResolution,
                Source = mediaData.Source,
                VideoCodec = mediaData.VideoCodec,
                AudioCodec = mediaData.AudioCodec,
                TmdbId = movie.Guid_TMDB,
                ImdbId = movie.Guid_IMDB,
                SeasonNumber = 0,
                EpisodeNumber = 0,
                TvdbId = 0,
            }.ToTorznabItem(
                command.Integration,
                command.TorznabApiKey,
                _networkSettings.Url,
                _appRuntimeInfo.IsDevelopmentEnvironment
            );
        }
    }
}
