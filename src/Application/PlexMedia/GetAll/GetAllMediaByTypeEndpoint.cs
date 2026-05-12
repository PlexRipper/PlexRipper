using FlexQuery.NET.Models;

namespace Reaparr.Application;

public record GetAllMediaByTypeRequest : MediaQueryFilterDTO;

public class GetAllMediaByTypeRequestValidator : Validator<GetAllMediaByTypeRequest>
{
    public GetAllMediaByTypeRequestValidator()
    {
        RuleFor(x => x.MediaType)
            .Must(type => type is PlexMediaType.TvShow or PlexMediaType.Movie)
            .WithMessage(x => $"Media type {x.MediaType} is not allowed.");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).When(x => x.Page.HasValue);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).When(x => x.PageSize.HasValue);
    }
}

public class GetAllMediaByTypeEndpoint : BaseEndpoint<GetAllMediaByTypeRequest, PlexMediaStatisticsDTO>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexMediaController;

    public GetAllMediaByTypeEndpoint(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<GetAllMediaByTypeEndpoint>();
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexMediaStatisticsDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetAllMediaByTypeRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var stopWatch = Stopwatch.StartNew();

        var mediaListResult = await _commandExecutor.Send(new GetMediaByTypeCommand
        {
            Filter = new MediaQueryFilter
            {
                MediaType = req.MediaType,
                PlexLibraryId = req.PlexLibraryId,
                FilterOfflineMedia = req.FilterOfflineMedia,
                FilterOwnedMedia = req.FilterOwnedMedia,
                Parameters = new FlexQueryParameters
                {
                    Query = req.Query,
                    Filter = req.Filter,
                    Sort = req.Sort,
                    Select = req.Select,
                    Includes = req.Includes,
                    GroupBy = req.GroupBy,
                    Having = req.Having,
                    Page = req.Page,
                    PageSize = req.PageSize,
                    IncludeCount = req.IncludeCount,
                    Distinct = req.Distinct,
                    Mode = req.Mode,
                },
            },
        }, ct);
        

        stopWatch.StopAndLog($"GetAllMediaByTypeEndpoint - Retrieved media with filter: {req}");

        if (mediaListResult.IsFailed)
        {
            await SendFluentResult(mediaListResult, ct);
            return;
        }

        await SendFluentResult(Result.Ok(ToStatisticsDTO(mediaListResult.Value)), ct);
    }
    
    public static PlexMediaStatisticsDTO ToStatisticsDTO(PagedMediaQueryResult source) => new()
    {
        QueryHash = source.QueryHash,
        Page = source.Page,
        PageSize = source.PageSize,
        MovieCount = source.MovieCount,
        TvShowCount = source.TvShowCount,
        SeasonCount = source.SeasonCount,
        EpisodeCount = source.EpisodeCount,
        TotalMovieCount = source.TotalMovieCount,
        TotalTvShowCount = source.TotalTvShowCount,
        TotalSeasonCount = source.TotalSeasonCount,
        TotalEpisodeCount = source.TotalEpisodeCount,
        MediaSize = source.MediaSize,
        TotalMediaSize = source.TotalMediaSize,
        MediaCount = source.TotalCount,
        MediaList = source.Items,
        NavigationIndexes = source.NavigationIndexes,
        Roles = source.Roles,
        Countries = source.Countries,
        Genres = source.Genres,
        Qualities = source.Qualities,
        TotalCount = source.TotalCount,
    };
}
