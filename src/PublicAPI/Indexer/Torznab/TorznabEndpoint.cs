using Reaparr.Application.Contracts;

namespace Reaparr.PublicAPI;

public class TorznabEndpointRequestValidator : Validator<TorznabEndpointRequest>
{
    public TorznabEndpointRequestValidator()
    {
        RuleFor(x => x.ParsedType).NotEqual(TorznabQueryType.Unknown);
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

    public override async Task HandleAsync(TorznabEndpointRequest endpointRequest, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, endpointRequest);
        var integration = HttpContext.GetIntegrationIdentity();
        var request = endpointRequest.ToTorznabRequest();

        switch (request.Mode)
        {
            case TorznabRequestMode.Capabilities:
                await SendCapabilitiesAsync(ct);
                break;
            case TorznabRequestMode.Rss:
                await SendMediaResultAsync(await GetRssAsync(request, integration, ct), ct);
                break;
            case TorznabRequestMode.ActiveSearch:
                await SendMediaResultAsync(await SearchAsync(request, integration, ct), ct);
                break;
            default:
                await Send.ErrorsAsync(cancellation: ct);
                break;
        }
    }

    private async Task SendCapabilitiesAsync(CancellationToken ct)
    {
        var result = await _commandExecutor.Send(new GetCapabilitiesCommand(), ct);
        if (result.IsFailed)
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.XmlAsync(result.Value, cancellationToken: ct);
    }

    private async Task SendMediaResultAsync(Result<TorznabMediaSearchResponseDTO> result, CancellationToken ct)
    {
        if (result.IsFailed)
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.XmlAsync(result.Value, cancellationToken: ct);
    }

    private Task<Result<TorznabMediaSearchResponseDTO>> GetRssAsync(
        TorznabRequest request,
        IntegrationIdentity integration,
        CancellationToken ct
    ) =>
        _commandExecutor.Send(
            new GetTorznabRssFeedCommand
            {
                Integration = integration,
                Categories = request.Categories,
                IncludeMovies = request.IncludesMovies,
                IncludeEpisodes = request.IncludesEpisodes,
                Limit = Math.Clamp(request.Limit, 0, 100),
                Offset = request.Offset,
                TorznabApiKey = request.ApiKey,
            },
            ct
        );

    private Task<Result<TorznabMediaSearchResponseDTO>> SearchAsync(
        TorznabRequest request,
        IntegrationIdentity integration,
        CancellationToken ct
    ) =>
        request.Type switch
        {
            TorznabQueryType.Search => GenericSearchAsync(request, integration, ct),
            TorznabQueryType.TvSearch => SearchTvAsync(request, integration, ct),
            TorznabQueryType.Movie => SearchMovieAsync(request, integration, ct),
            _ => Task.FromResult(Result.Fail<TorznabMediaSearchResponseDTO>("Unsupported Torznab search type")),
        };

    private Task<Result<TorznabMediaSearchResponseDTO>> SearchTvAsync(
        TorznabRequest request,
        IntegrationIdentity integration,
        CancellationToken ct
    ) =>
        _commandExecutor.Send(
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

    private Task<Result<TorznabMediaSearchResponseDTO>> SearchMovieAsync(
        TorznabRequest request,
        IntegrationIdentity integration,
        CancellationToken ct
    ) =>
        _commandExecutor.Send(
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

    /// <summary>
    /// Handles generic active searches by running the TV and movie searches requested by categories.
    /// </summary>
    private async Task<Result<TorznabMediaSearchResponseDTO>> GenericSearchAsync(
        TorznabRequest request,
        IntegrationIdentity integration,
        CancellationToken ct
    )
    {
        var items = new List<TorznabItem>();
        var failures = new List<IError>();
        var successfulSearches = 0;

        if (request.IncludesEpisodes)
        {
            var tvResult = await SearchTvAsync(request with { Season = 0, Episode = 0 }, integration, ct);
            if (tvResult.IsFailed)
                failures.AddRange(tvResult.Errors);
            else
            {
                successfulSearches++;
                items.AddRange(tvResult.Value.Channel.Items);
            }
        }

        if (request.IncludesMovies)
        {
            var movieResult = await SearchMovieAsync(request, integration, ct);
            if (movieResult.IsFailed)
                failures.AddRange(movieResult.Errors);
            else
            {
                successfulSearches++;
                items.AddRange(movieResult.Value.Channel.Items);
            }
        }

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
                    Description = $"Search results for {request.Query}",
                    Items = items.Take(request.Limit).ToList(),
                },
            }
        );
    }
}
