using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// The request to clear any completed <see cref="DownloadTaskGeneric"/> from the database.
/// </summary>
public class ClearCompletedDownloadTasksRequest
{
    /// <summary>
    /// Optional: The list of only these <see cref="DownloadTaskGeneric"/> id's to delete.
    /// </summary>
    [FromBody]
    public List<Guid> DownloadTaskIds { get; init; }
}

/// <summary>
/// Will clear any completed <see cref="DownloadTaskGeneric"/> from the database.
/// </summary>
/// <returns>Is successful.</returns>
public class ClearCompletedDownloadTasksEndpoint : BaseCustomEndpoint<ClearCompletedDownloadTasksRequest, ResultDTO>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.DownloadController + "/clear";

    public ClearCompletedDownloadTasksEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Verbs(Http.POST);
        Post(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(ClearCompletedDownloadTasksRequest req, CancellationToken ct)
    {
        var hasDownloadTaskIds = req.DownloadTaskIds != null && req.DownloadTaskIds.Any();

        await _dbContext.DownloadTaskMovie
            .Where(x => (!hasDownloadTaskIds || req.DownloadTaskIds.Contains(x.Id)) && x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(ct);

        await _dbContext.DownloadTaskMovieFile
            .Where(x => (!hasDownloadTaskIds || req.DownloadTaskIds.Contains(x.Id)) && x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(ct);

        await _dbContext.DownloadTaskTvShow
            .Where(x => (!hasDownloadTaskIds || req.DownloadTaskIds.Contains(x.Id)) && x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(ct);

        await _dbContext.DownloadTaskTvShowSeason
            .Where(x => (!hasDownloadTaskIds || req.DownloadTaskIds.Contains(x.Id)) && x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(ct);

        await _dbContext.DownloadTaskTvShowEpisode
            .Where(x => (!hasDownloadTaskIds || req.DownloadTaskIds.Contains(x.Id)) && x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(ct);

        await _dbContext.DownloadTaskTvShowEpisodeFile
            .Where(x => (!hasDownloadTaskIds || req.DownloadTaskIds.Contains(x.Id)) && x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(ct);

        await SendResult(Result.Ok(), ct);
    }
}