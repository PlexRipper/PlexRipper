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
        var orphanParentCandidates = await GetOrphanParentCandidatesAsync(affectedRootIds, ct);

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

        var orphanDeletedParentKeys = await GetDeletedParentKeysAsync(orphanParentCandidates, ct);

        // Notify front-end as well
        var notified = new HashSet<(Guid Id, DownloadTaskType Type)>();
        foreach (var key in command.Keys)
        {
            await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(key, DownloadStatus.Deleted, ct);
            notified.Add((key.Id, key.Type));
        }

        foreach (var parentKey in orphanDeletedParentKeys)
        {
            if (!notified.Contains((parentKey.Id, parentKey.Type)))
                await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(parentKey, DownloadStatus.Deleted, ct);
        }

        return Result.Ok();
    }

    private async Task<List<DownloadTaskKey>> GetOrphanParentCandidatesAsync(
        IReadOnlyCollection<Guid> rootIds,
        CancellationToken ct
    )
    {
        if (rootIds.Count == 0)
            return [];

        var movieCandidatesTask = _dbContext
            .DownloadTaskMovie.Where(x => rootIds.Contains(x.Id))
            .Select(x => new DownloadTaskKey
            {
                Id = x.Id,
                Type = DownloadTaskType.Movie,
                PlexServerId = x.PlexServerId,
                PlexLibraryId = x.PlexLibraryId,
            })
            .ToListAsync(ct);

        var tvShowCandidatesTask = _dbContext
            .DownloadTaskTvShow.Where(x => rootIds.Contains(x.Id))
            .Select(x => new DownloadTaskKey
            {
                Id = x.Id,
                Type = DownloadTaskType.TvShow,
                PlexServerId = x.PlexServerId,
                PlexLibraryId = x.PlexLibraryId,
            })
            .ToListAsync(ct);

        var seasonCandidatesTask = _dbContext
            .DownloadTaskTvShowSeason.Where(x => rootIds.Contains(x.ParentId))
            .Select(x => new DownloadTaskKey
            {
                Id = x.Id,
                Type = DownloadTaskType.Season,
                PlexServerId = x.PlexServerId,
                PlexLibraryId = x.PlexLibraryId,
            })
            .ToListAsync(ct);

        var episodeCandidatesTask = _dbContext
            .DownloadTaskTvShowEpisode.Where(x => rootIds.Contains(x.Parent!.ParentId))
            .Select(x => new DownloadTaskKey
            {
                Id = x.Id,
                Type = DownloadTaskType.Episode,
                PlexServerId = x.PlexServerId,
                PlexLibraryId = x.PlexLibraryId,
            })
            .ToListAsync(ct);

        await Task.WhenAll(movieCandidatesTask, tvShowCandidatesTask, seasonCandidatesTask, episodeCandidatesTask);

        return movieCandidatesTask
            .Result.Concat(tvShowCandidatesTask.Result)
            .Concat(seasonCandidatesTask.Result)
            .Concat(episodeCandidatesTask.Result)
            .GroupBy(k => new
            {
                k.Id,
                k.Type,
                k.PlexServerId,
                k.PlexLibraryId,
            })
            .Select(g => g.First())
            .ToList();
    }

    private async Task<List<DownloadTaskKey>> GetDeletedParentKeysAsync(
        IReadOnlyCollection<DownloadTaskKey> candidates,
        CancellationToken ct
    )
    {
        if (candidates.Count == 0)
            return [];

        var idsByType = candidates.GroupBy(x => x.Type).ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

        var existing = new HashSet<Guid>();

        if (idsByType.TryGetValue(DownloadTaskType.Movie, out var movieIds) && movieIds.Count > 0)
        {
            var matches = await _dbContext
                .DownloadTaskMovie.Where(x => movieIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(ct);
            existing.UnionWith(matches);
        }

        if (idsByType.TryGetValue(DownloadTaskType.TvShow, out var tvShowIds) && tvShowIds.Count > 0)
        {
            var matches = await _dbContext
                .DownloadTaskTvShow.Where(x => tvShowIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(ct);
            existing.UnionWith(matches);
        }

        if (idsByType.TryGetValue(DownloadTaskType.Season, out var seasonIds) && seasonIds.Count > 0)
        {
            var matches = await _dbContext
                .DownloadTaskTvShowSeason.Where(x => seasonIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(ct);
            existing.UnionWith(matches);
        }

        if (idsByType.TryGetValue(DownloadTaskType.Episode, out var episodeIds) && episodeIds.Count > 0)
        {
            var matches = await _dbContext
                .DownloadTaskTvShowEpisode.Where(x => episodeIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(ct);
            existing.UnionWith(matches);
        }

        return candidates.Where(x => !existing.Contains(x.Id)).ToList();
    }
}
