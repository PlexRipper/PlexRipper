using System.Text;
using System.Xml.Serialization;
using FastEndpoints;
using FluentValidation;

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
        Description(x => x.IsIndexer());
        AllowAnonymous();
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
                await Send.XMLAsync(response, cancellationToken: ct);
                break;
            case "search":
                throw new NotImplementedException();
            case "tvsearch":
                var searchTvShowResponse = await _commandExecutor.Send(new SearchTvShowCommand
                {
                    Query = req.Query,
                    Season = req.Season,
                    Episode = req.Episode,
                    TVDB_ID = req.TvdbId,
                    IMDB_ID = req.ImdbId,
                    TMDB_ID = req.TmdbId,
                    Limit = req.Limit,
                    Offset = req.Offset,
                }, ct);
                await Send.XMLAsync(searchTvShowResponse, cancellationToken: ct);
                break;
            case "movie":
                throw new NotImplementedException();
        }
    }
}