using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Will clear any completed <see cref="DownloadTaskGeneric"/> from the database.
/// </summary>
/// <param name="DownloadTaskIds"> The list of <see cref="DownloadTaskGeneric"/> id's to delete.</param>
/// <returns>Is successful.</returns>
public record ClearCompletedDownloadTasksCommand(List<Guid> DownloadTaskIds) : IRequest<Result>;

public class ClearCompletedDownloadTasksCommandValidator : AbstractValidator<ClearCompletedDownloadTasksCommand> { }

public class ClearCompletedDownloadTasksHandler :
    IRequestHandler<ClearCompletedDownloadTasksCommand, Result>
{
    private readonly IPlexRipperDbContext _dbContext;

    public ClearCompletedDownloadTasksHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(ClearCompletedDownloadTasksCommand command, CancellationToken cancellationToken)
    {
        var hasDownloadTaskIds = command.DownloadTaskIds != null && command.DownloadTaskIds.Any();

        await _dbContext.DownloadTaskMovie
            .Where(x => (!hasDownloadTaskIds || command.DownloadTaskIds.Contains(x.Id)) && x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.DownloadTaskMovieFile
            .Where(x => (!hasDownloadTaskIds || command.DownloadTaskIds.Contains(x.Id)) && x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.DownloadTaskTvShow
            .Where(x => (!hasDownloadTaskIds || command.DownloadTaskIds.Contains(x.Id)) && x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.DownloadTaskTvShowSeason
            .Where(x => (!hasDownloadTaskIds || command.DownloadTaskIds.Contains(x.Id)) && x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.DownloadTaskTvShowEpisode
            .Where(x => (!hasDownloadTaskIds || command.DownloadTaskIds.Contains(x.Id)) && x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.DownloadTaskTvShowEpisodeFile
            .Where(x => (!hasDownloadTaskIds || command.DownloadTaskIds.Contains(x.Id)) && x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(cancellationToken);

        return Result.Ok();
    }
}