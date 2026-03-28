using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public class DeleteDownloadTasksByKeyCommandValidator : AbstractValidator<DeleteDownloadTasksByKeyCommand>
{
    public DeleteDownloadTasksByKeyCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.Keys).NotEmpty();
    }
}

public class DeleteDownloadTasksByKeyCommandHandler : ICommandHandler<DeleteDownloadTasksByKeyCommand, Result>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly IDownloadTaskUpdateDispatcher _downloadTaskUpdateDispatcher;

    public DeleteDownloadTasksByKeyCommandHandler(
        IReaparrDbContext dbContext,
        IDownloadTaskUpdateDispatcher downloadTaskUpdateDispatcher
    )
    {
        _dbContext = dbContext;
        _downloadTaskUpdateDispatcher = downloadTaskUpdateDispatcher;
    }

    public async Task<Result> ExecuteAsync(DeleteDownloadTasksByKeyCommand command, CancellationToken ct)
    {
        var affectedRootIds = await _dbContext.GetAffectedRootDownloadTaskIdsAsync(
            command.Keys.Select(k => k.Id).ToList(),
            ct
        );

        // Group by type → one targeted DELETE per table, no scatter across all six.
        var byType = command.Keys.ToLookup(k => k.Type, k => k.Id);

        var movieIds = byType[DownloadTaskType.Movie].ToList();
        var movieFileIds = byType[DownloadTaskType.MovieData].Concat(byType[DownloadTaskType.MoviePart]).ToList();
        var tvShowIds = byType[DownloadTaskType.TvShow].ToList();
        var seasonIds = byType[DownloadTaskType.Season].ToList();
        var episodeIds = byType[DownloadTaskType.Episode].ToList();
        var episodeFileIds = byType[DownloadTaskType.EpisodeData].Concat(byType[DownloadTaskType.EpisodePart]).ToList();

        if (movieIds.Count > 0)
            await _dbContext.DownloadTaskMovie.Where(x => movieIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        if (movieFileIds.Count > 0)
            await _dbContext.DownloadTaskMovieFile.Where(x => movieFileIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        if (tvShowIds.Count > 0)
            await _dbContext.DownloadTaskTvShow.Where(x => tvShowIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        if (seasonIds.Count > 0)
            await _dbContext.DownloadTaskTvShowSeason.Where(x => seasonIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        if (episodeIds.Count > 0)
            await _dbContext.DownloadTaskTvShowEpisode.Where(x => episodeIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        if (episodeFileIds.Count > 0)
            await _dbContext
                .DownloadTaskTvShowEpisodeFile.Where(x => episodeFileIds.Contains(x.Id))
                .ExecuteDeleteAsync(ct);

        // Exclude roots that were already directly deleted above — orphan cleanup only
        // applies to roots whose children were removed, not roots deleted explicitly.
        var directlyDeletedRootIds = new HashSet<Guid>(movieIds.Concat(tvShowIds));
        var orphanRootIds = affectedRootIds.Where(id => !directlyDeletedRootIds.Contains(id)).ToList();
        await _dbContext.DeleteOrphanedParentTasksByRootIdsAsync(orphanRootIds, ct);

        // Notify front-end as well
        foreach (var key in command.Keys)
            await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(key, DownloadStatus.Deleted, ct);

        return Result.Ok();
    }
}
