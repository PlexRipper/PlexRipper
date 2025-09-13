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
    private readonly ICommandExecutor _commandExecutor;

    public override void Configure()
    {
        Get(PublicApiRoutes.Indexer);
        Description(x => x.IsIndexer());
        AllowAnonymous();
    }

    public TorznabEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override async Task HandleAsync(TorznabEndpointRequest req, CancellationToken ct)
    {
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
                    TVDB_ID = req.TvdbId
                }, ct);
                await Send.XMLAsync(searchTvShowResponse, cancellationToken: ct);
                break;
            case "movie":
                throw new NotImplementedException();
        }
    }
}