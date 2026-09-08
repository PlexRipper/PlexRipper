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
                switch (integration.Type)
                {
                    case IntegrationType.Sonarr:
                    {
                        var genericTvSearchResult = await _commandExecutor.Send(
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
                        if (genericTvSearchResult.IsFailed)
                        {
                            await Send.ErrorsAsync(cancellation: ct);
                            break;
                        }
                        await Send.XmlAsync(genericTvSearchResult.Value, cancellationToken: ct);
                        break;
                    }
                    case IntegrationType.Radarr:
                    {
                        var genericMovieSearchResult = await _commandExecutor.Send(
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
                        if (genericMovieSearchResult.IsFailed)
                        {
                            await Send.ErrorsAsync(cancellation: ct);
                            break;
                        }
                        await Send.XmlAsync(genericMovieSearchResult.Value, cancellationToken: ct);
                        break;
                    }
                    default:
                        await Send.ForbiddenAsync(ct);
                        break;
                }
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
}
