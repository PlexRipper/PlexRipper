using Reaparr.Application.Contracts;

namespace Reaparr.PublicAPI;

public class TorznabEndpointRequestValidator : Validator<TorznabEndpointRequest>
{
    public TorznabEndpointRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(type => new[] { "caps", "search", "tvsearch", "movie" }.Contains(type))
            .WithMessage("Type must be one of: caps, search, tvsearch, movie");
        RuleFor(x => x.ApiKey).NotEmpty();
    }
}

public sealed class TorznabEndpoint : Endpoint<TorznabEndpointRequest>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public TorznabEndpoint(ILogger logger, ICommandExecutor commandExecutor)
    {
        _log = logger.ForContext<TorznabEndpoint>();
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(PublicApiRoutes.Indexer);
        Description(x =>
        {
            x.IsIndexer();
            x.Produces<BaseResultDTO>();
            x.Produces<BaseResultDTO>(StatusCodes.Status401Unauthorized);
            x.Produces<BaseResultDTO>(StatusCodes.Status403Forbidden);
            x.Produces<BaseResultDTO>(StatusCodes.Status500InternalServerError);
        });
        AllowAnonymous();
        PreProcessor<IndexerAuthenticationPreProcessor<TorznabEndpointRequest>>();
    }

    public override async Task HandleAsync(TorznabEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        var integration = HttpContext.GetIntegrationIdentity();
        if (
            (req.Type == "movie" && !integration.Supports(PlexMediaType.Movie))
            || (req.Type == "tvsearch" && !integration.Supports(PlexMediaType.Episode))
        )
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        switch (req.Type)
        {
            case "caps":
                var capsResult = await _commandExecutor.Send(new GetCapabilitiesCommand(), ct);
                if (capsResult.IsFailed)
                {
                    await Send.ErrorsAsync(cancellation: ct);
                    break;
                }
                await Send.XmlAsync(capsResult.Value, cancellationToken: ct);
                break;
            case "search":
                var searchResult = await GenericSearchAsync(req, integration, ct);
                if (searchResult.IsFailed)
                {
                    await Send.ErrorsAsync(cancellation: ct);
                    break;
                }
                await Send.XmlAsync(searchResult.Value, cancellationToken: ct);
                break;
            case "tvsearch":
                var tvSearchResult = await _commandExecutor.Send(
                    new SearchTvShowCommand
                    {
                        Query = req.Query ?? string.Empty,
                        Season = req.Season ?? 0,
                        Episode = req.Episode ?? 0,
                        TVDB_ID = req.TvdbId ?? 0,
                        IMDB_ID = req.ImdbId ?? string.Empty,
                        TMDB_ID = req.TmdbId ?? 0,
                        Limit = req.Limit ?? 100,
                        Offset = req.Offset ?? 0,
                        Integration = integration,
                        TorznabApiKey = req.ApiKey,
                    },
                    ct
                );
                if (tvSearchResult.IsFailed)
                {
                    await Send.ErrorsAsync(cancellation: ct);
                    break;
                }
                await Send.XmlAsync(tvSearchResult.Value, cancellationToken: ct);
                break;
            case "movie":
                var movieSearchResult = await _commandExecutor.Send(
                    new SearchMovieCommand
                    {
                        Query = req.Query ?? string.Empty,
                        IMDB_ID = req.ImdbId ?? string.Empty,
                        TMDB_ID = req.TmdbId ?? 0,
                        Limit = req.Limit ?? 100,
                        Offset = req.Offset ?? 0,
                        Integration = integration,
                        TorznabApiKey = req.ApiKey,
                    },
                    ct
                );
                if (movieSearchResult.IsFailed)
                {
                    await Send.ErrorsAsync(cancellation: ct);
                    break;
                }
                await Send.XmlAsync(movieSearchResult.Value, cancellationToken: ct);
                break;
            default:
                _log.Here().Error("Received unknown Torznab request type: {Type}", req.Type);
                await Send.ErrorsAsync(cancellation: ct);
                break;
        }
    }

    /// <summary>
    /// Handles the generic "search" query type by running the TV and movie searches the
    /// requested categories ask for and merging their results.
    ///
    /// Sonarr and Radarr fall back to t=search for some searches, so this cannot simply be
    /// unimplemented: an error response here is counted as an indexer failure and the *arr
    /// eventually marks the whole indexer unavailable, which also silences t=tvsearch and
    /// t=movie.
    /// </summary>
    private async Task<Result<TorznabMediaSearchResponseDTO>> GenericSearchAsync(
        TorznabEndpointRequest req,
        IntegrationIdentity integration,
        CancellationToken ct
    )
    {
        var categories = req.Categories ?? [];
        var wantsTvShows = categories.Length == 0 || categories.Any(IsTvShowCategory);
        var wantsMovies = categories.Length == 0 || categories.Any(IsMovieCategory);

        var limit = req.Limit ?? 100;
        var offset = req.Offset ?? 0;
        var query = req.Query ?? string.Empty;

        var items = new List<TorznabItem>();
        var failures = new List<IError>();
        var successfulSearches = 0;

        if (wantsTvShows)
        {
            var tvResult = await _commandExecutor.Send(
                new SearchTvShowCommand
                {
                    Query = query,
                    Season = 0,
                    Episode = 0,
                    TVDB_ID = req.TvdbId ?? 0,
                    IMDB_ID = req.ImdbId ?? string.Empty,
                    TMDB_ID = req.TmdbId ?? 0,
                    Limit = limit,
                    Offset = offset,
                    Integration = integration,
                    TorznabApiKey = req.ApiKey,
                },
                ct
            );

            if (tvResult.IsFailed)
                failures.AddRange(tvResult.Errors);
            else
            {
                successfulSearches++;
                items.AddRange(tvResult.Value.Channel.Items);
            }
        }

        if (wantsMovies)
        {
            var movieResult = await _commandExecutor.Send(
                new SearchMovieCommand
                {
                    Query = query,
                    IMDB_ID = req.ImdbId ?? string.Empty,
                    TMDB_ID = req.TmdbId ?? 0,
                    Limit = limit,
                    Offset = offset,
                    Integration = integration,
                    TorznabApiKey = req.ApiKey,
                },
                ct
            );

            if (movieResult.IsFailed)
                failures.AddRange(movieResult.Errors);
            else
            {
                successfulSearches++;
                items.AddRange(movieResult.Value.Channel.Items);
            }
        }

        // Only fail when nothing could be searched at all - a partial result is still far
        // better than an error the *arr will hold against the indexer.
        if (failures.Count > 0 && successfulSearches == 0)
            return Result.Fail(failures);

        if (failures.Count > 0)
            _log.Here().Warning("Generic Torznab search partially failed: {Errors}", failures);

        return Result.Ok(
            new TorznabMediaSearchResponseDTO
            {
                Channel = new TorznabChannel
                {
                    Title = "Reaparr Indexer",
                    Description = $"Search results for {query}",
                    Items = items.Take(limit).ToList(),
                },
            }
        );
    }

    private static bool IsTvShowCategory(int category) => category is >= 5000 and < 6000;

    private static bool IsMovieCategory(int category) => category is >= 2000 and < 3000;
}
