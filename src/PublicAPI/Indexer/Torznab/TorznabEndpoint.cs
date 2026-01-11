using FastEndpoints;
using FluentValidation;
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
    }
}

public sealed class TorznabEndpoint : Endpoint<TorznabEndpointRequest>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

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

    public TorznabEndpoint(ILogger logger, ICommandExecutor commandExecutor)
    {
        _log = logger.ForContext<TorznabEndpoint>();
        _commandExecutor = commandExecutor;
    }

    public override async Task HandleAsync(TorznabEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        switch (req.Type)
        {
            case "caps":
                var response = await _commandExecutor.Send(new GetCapabilitiesCommand(), ct);
                await Send.XmlAsync(response, cancellationToken: ct);
                break;
            case "search":
                throw new NotImplementedException();
            case "tvsearch":
                var searchTvShowResponse = await _commandExecutor.Send(
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
                    },
                    ct
                );
                await Send.XmlAsync(searchTvShowResponse, cancellationToken: ct);
                break;
            case "movie":
                var searchMovieResponse = await _commandExecutor.Send(
                    new SearchMovieCommand
                    {
                        Query = req.Query ?? string.Empty,
                        IMDB_ID = req.ImdbId ?? string.Empty,
                        TMDB_ID = req.TmdbId ?? 0,
                        Limit = req.Limit ?? 100,
                        Offset = req.Offset ?? 0,
                    },
                    ct
                );
                await Send.XmlAsync(searchMovieResponse, cancellationToken: ct);
                break;
            default:
                _log.Here().Error("Received unknown Torznab request type: {Type}", req.Type);
                await Send.ErrorsAsync(cancellation: ct);
                break;
        }
    }
}
