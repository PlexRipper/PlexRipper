using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.PublicAPI.SearchTvShow;

namespace Reaparr.PublicAPI;

public class TorznabEndpointRequestValidator : Validator<TorznabEndpointRequest>
{
    public TorznabEndpointRequestValidator()
    {
        RuleFor(x => x.Type).NotEmpty();
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
                var searchTvShowResponse = await _commandExecutor.Send(new SearchTvShowCommand
                {
                    Query = req.Query,
                    Season = req.Season ?? 0,
                    Episode = req.Episode ?? 0,
                    TVDB_ID = req.TvdbId ?? 0,
                    IMDB_ID = req.ImdbId ?? string.Empty,
                    TMDB_ID = req.TmdbId ?? 0,
                    Limit = req.Limit ?? 100,
                    Offset = req.Offset ?? 0,
                }, ct);
                await Send.XmlAsync(searchTvShowResponse, cancellationToken: ct);
                break;
            case "movie":
                throw new NotImplementedException();
        }
    }
}