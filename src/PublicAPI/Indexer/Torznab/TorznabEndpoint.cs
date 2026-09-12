using Reaparr.Application.Contracts;

namespace Reaparr.PublicAPI;

public class TorznabEndpointRequestValidator : Validator<TorznabEndpointRequest>
{
    public TorznabEndpointRequestValidator()
    {
        RuleFor(x => x.ParsedType)
            .Must(type => type != TorznabQueryType.Unknown)
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
        var request = req.ToTorznabRequest();

        if (request.Type != TorznabQueryType.Caps && request.IsRssSync)
        {
            var rssResult = await _commandExecutor.Send(
                new GetTorznabRssFeedCommand
                {
                    Integration = integration,
                    Categories = request.Categories,
                    IncludeMovies = request.HasMovieCategory,
                    IncludeEpisodes = request.HasTvShowCategory,
                    Limit = request.Limit,
                    Offset = request.Offset,
                    TorznabApiKey = request.ApiKey,
                },
                ct
            );
            if (rssResult.IsFailed)
            {
                await Send.ErrorsAsync(cancellation: ct);
                return;
            }

            await Send.XmlAsync(rssResult.Value, cancellationToken: ct);
            return;
        }

        switch (request.Type)
        {
            case TorznabQueryType.Caps:
                var capsResult = await _commandExecutor.Send(new GetCapabilitiesCommand(), ct);
                if (capsResult.IsFailed)
                {
                    await Send.ErrorsAsync(cancellation: ct);
                    break;
                }

                await Send.XmlAsync(capsResult.Value, cancellationToken: ct);
                break;
            case TorznabQueryType.Search:
                var searchResult = await GenericSearchAsync(request, integration, ct);
                if (searchResult.IsFailed)
                {
                    await Send.ErrorsAsync(cancellation: ct);
                    break;
                }

                await Send.XmlAsync(searchResult.Value, cancellationToken: ct);
                break;
            case TorznabQueryType.TvSearch:
                var tvSearchResult = await _commandExecutor.Send(
                    new SearchTvShowCommand
                    {
                        Query = request.Query,
                        Season = request.Season,
                        Episode = request.Episode,
                        TVDB_ID = request.TvdbId,
                        IMDB_ID = request.ImdbId,
                        TMDB_ID = request.TmdbId,
                        Limit = request.Limit,
                        Offset = request.Offset,
                        Integration = integration,
                        TorznabApiKey = request.ApiKey,
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
            case TorznabQueryType.Movie:
                var movieSearchResult = await _commandExecutor.Send(
                    new SearchMovieCommand
                    {
                        Query = request.Query,
                        IMDB_ID = request.ImdbId,
                        TMDB_ID = request.TmdbId,
                        Limit = request.Limit,
                        Offset = request.Offset,
                        Integration = integration,
                        TorznabApiKey = request.ApiKey,
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
            case TorznabQueryType.Unknown:
            default:
                _log.Here().Error("Received unknown Torznab request type: {Type}", request.Type);
                await Send.ErrorsAsync(cancellation: ct);
                break;
        }
    }

    /// <summary>
    /// Handles the generic "search" query type by running the TV and movie searches the
    /// requested categories ask for and merging their results.
    /// Sonarr and Radarr fall back to t=search for some searches, so this cannot simply be
    /// unimplemented: an error response here is counted as an indexer failure and the *arr
    /// eventually marks the whole indexer unavailable, which also silences t=tvsearch and
    /// t=movie.
    /// </summary>
    private async Task<Result<TorznabMediaSearchResponseDTO>> GenericSearchAsync(
        TorznabRequest req,
        IntegrationIdentity integration,
        CancellationToken ct
    )
    {
        var items = new List<TorznabItem>();
        var failures = new List<IError>();
        var successfulSearches = 0;

        if (req.HasTvShowCategory)
        {
            var tvResult = await _commandExecutor.Send(
                new SearchTvShowCommand
                {
                    Query = req.Query,
                    Season = 0,
                    Episode = 0,
                    TVDB_ID = req.TvdbId,
                    IMDB_ID = req.ImdbId,
                    TMDB_ID = req.TmdbId,
                    Limit = req.Limit,
                    Offset = req.Offset,
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

        if (req.HasMovieCategory)
        {
            var movieResult = await _commandExecutor.Send(
                new SearchMovieCommand
                {
                    Query = req.Query,
                    IMDB_ID = req.ImdbId,
                    TMDB_ID = req.TmdbId,
                    Limit = req.Limit,
                    Offset = req.Offset,
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
                    Description = $"Search results for {req.Query}",
                    Items = items.Take(req.Limit).ToList(),
                },
            }
        );
    }
}
