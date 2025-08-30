using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record GetAllSubtitlesEndpointRequest
{
    [QueryParam, BindFrom("type")]
    public required PlexMediaType Type { get; init; } = PlexMediaType.TvShow;

    [QueryParam, BindFrom("count")]
    public int? Count { get; init; } = 0;
}

public class GetAllSubtitlesEndpoint : BaseEndpoint<GetAllSubtitlesEndpointRequest, List<string>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.DebugController + "/get-all-subtitles";

    public GetAllSubtitlesEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<string>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetAllSubtitlesEndpointRequest req, CancellationToken ct)
    {
        if (req.Type == PlexMediaType.Movie)
        {
            var subtitles = await _dbContext
                .PlexMovieDataStreams.Where(x => x.StreamType == StreamType.Subtitle)
                .ToListAsync(ct);
            await SendFluentResult(Result.Ok(subtitles), ct);
            return;
        }

        if (req.Type == PlexMediaType.TvShow)
        {
            var subtitles = await _dbContext
                .PlexTvShowEpisodeDataStreams.Where(x => x.StreamType == StreamType.Subtitle)
                .ToListAsync(ct);
            await SendFluentResult(Result.Ok(subtitles), ct);
        }
    }
}
