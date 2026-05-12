using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Reaparr.Application;

/// <summary>
/// Refresh a single download task's Plex identifiers (RatingKey / MediaId / PartId / FileLocationUrl)
/// against the currently-synced library data. Used to recover from stale identifiers when the source
/// Plex server has been re-scanned and the task's previously-cached IDs no longer resolve. Returns
/// <c>true</c> if the task was updated, <c>false</c> if no change was needed or possible.
/// </summary>
public record RefreshDownloadTaskMetadataCommand(Guid DownloadTaskFileId, DownloadTaskType TaskType)
    : ICommand<Result<bool>>;

/// <summary>
/// Helpers for the "on 404, try refreshing the task's Plex IDs once before giving up" pattern,
/// shared between every download client that surfaces a Plex 404 (Direct probe + post-download,
/// Dash, and the DownloadJob fall-through). Returns <c>true</c> if the refresh succeeded and the
/// caller should treat the task as recoverable; <c>false</c> means fall through to the existing
/// failure handling.
/// </summary>
public static class DownloadTaskRefreshHelper
{
    public static async Task<bool> TryRefreshIfStaleIdAsync(
        this ICommandExecutor commandExecutor,
        Guid downloadTaskFileId,
        DownloadTaskType taskType,
        Result failureResult,
        CancellationToken cancellationToken
    )
    {
        if (!failureResult.Has404NotFoundError())
            return false;

        if (taskType is not DownloadTaskType.EpisodeData and not DownloadTaskType.MovieData)
            return false;

        var refreshResult = await commandExecutor.Send(
            new RefreshDownloadTaskMetadataCommand(downloadTaskFileId, taskType),
            cancellationToken
        );

        return refreshResult.IsSuccess && refreshResult.Value;
    }
}

public class RefreshDownloadTaskMetadataCommandValidator : AbstractValidator<RefreshDownloadTaskMetadataCommand>
{
    public RefreshDownloadTaskMetadataCommandValidator()
    {
        RuleFor(x => x.DownloadTaskFileId).NotEqual(Guid.Empty);
        RuleFor(x => x.TaskType)
            .Must(t => t == DownloadTaskType.EpisodeData || t == DownloadTaskType.MovieData)
            .WithMessage("Only EpisodeData and MovieData tasks can be refreshed.");
    }
}

public class RefreshDownloadTaskMetadataCommandHandler
    : ICommandHandler<RefreshDownloadTaskMetadataCommand, Result<bool>>
{
    /// <summary>
    /// Suppress repeated refresh attempts for the same task within this window. Refreshing is
    /// cheap (one indexed lookup) but Quartz could otherwise re-fire the queue picker for the
    /// same broken task many times per minute and produce noisy log churn.
    /// </summary>
    internal static readonly TimeSpan RefreshCooldown = TimeSpan.FromMinutes(15);

    // S03E15, S03.E15, S 03 E 15, etc.
    private static readonly Regex SeRegexCombined = new(
        @"[Ss](\d{1,3})[._\s]?[Ee](\d{1,3})",
        RegexOptions.Compiled
    );

    // 23x01 — used by some Top Gear releases.
    private static readonly Regex SeRegexXShort = new(@"\b(\d{1,3})x(\d{1,3})\b", RegexOptions.Compiled);

    // Season number from a path segment like ".../Season 2/...".
    private static readonly Regex SeasonPathRegex = new(@"Season\s+(\d{1,3})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Episode-only fallback when only the season was on the path.
    private static readonly Regex EpisodeOnlyRegex = new(
        @"\b(?:E|EP|Episode)[._\s]?(\d{1,3})\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    internal static readonly ConcurrentDictionary<Guid, DateTime> _lastRefreshAt = new();

    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly IDownloadQueue _downloadQueue;

    public RefreshDownloadTaskMetadataCommandHandler(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        IDownloadQueue downloadQueue
    )
    {
        _log = log.ForContext<RefreshDownloadTaskMetadataCommandHandler>();
        _dbContextFactory = dbContextFactory;
        _downloadQueue = downloadQueue;
    }

    public async Task<Result<bool>> ExecuteAsync(
        RefreshDownloadTaskMetadataCommand command,
        CancellationToken cancellationToken
    )
    {
        if (
            _lastRefreshAt.TryGetValue(command.DownloadTaskFileId, out var last)
            && DateTime.UtcNow - last < RefreshCooldown
        )
        {
            return Result.Ok(false);
        }

        _lastRefreshAt[command.DownloadTaskFileId] = DateTime.UtcNow;

        return command.TaskType switch
        {
            DownloadTaskType.EpisodeData => await RefreshEpisodeAsync(command.DownloadTaskFileId, cancellationToken),
            DownloadTaskType.MovieData => await RefreshMovieAsync(command.DownloadTaskFileId, cancellationToken),
            _ => Result.Ok(false),
        };
    }

    private async Task<Result<bool>> RefreshEpisodeAsync(Guid taskId, CancellationToken ct)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        // Load the file + parent show context. AsNoTracking because we apply changes via ExecuteUpdate.
        var task = await dbContext
            .DownloadTaskTvShowEpisodeFile.AsNoTracking()
            .Where(f => f.Id == taskId)
            .Select(f => new
            {
                f.Id,
                f.PlexServerId,
                f.PlexLibraryId,
                f.Quality,
                f.PlexApiRatingKey,
                f.PlexApiMediaId,
                f.PlexApiPartId,
                f.FileLocationUrl,
                f.FullTitle,
                ShowTitle = f.Parent!.Parent!.Parent!.Title,
                ShowYear = f.Parent!.Parent!.Parent!.Year,
            })
            .FirstOrDefaultAsync(ct);

        if (task is null)
            return Result.Ok(false);

        var parsed = ParseSeasonEpisode(task.FullTitle);
        if (parsed is null)
        {
            _log.Here()
                .Warning(
                    "Refresh skipped — could not parse season/episode from FullTitle: {FullTitle}",
                    task.FullTitle
                );
            return Result.Ok(false);
        }
        var (seasonNumber, episodeNumber) = parsed.Value;

        // Match the freshly-synced library data. Prefer same Quality, fall back to highest available.
        var candidates = await (
            from ed in dbContext.PlexTvShowEpisodeData.AsNoTracking()
            join e in dbContext.PlexTvShowEpisodes on ed.PlexTvShowEpisodeId equals e.Id
            join s in dbContext.PlexTvShowSeason on e.TvShowSeasonId equals s.Id
            join sh in dbContext.PlexTvShows on s.TvShowId equals sh.Id
            where sh.PlexServerId == task.PlexServerId
                && sh.PlexLibraryId == task.PlexLibraryId
                && sh.Title == task.ShowTitle
                && sh.Year == task.ShowYear
                && s.SeasonNumber == seasonNumber
                && e.EpisodeNumber == episodeNumber
            select new
            {
                ed.PlexApiRatingKey,
                ed.PlexApiMediaId,
                ed.PlexApiPartId,
                ed.Key,
                ed.Quality,
            }
        ).ToListAsync(ct);

        if (candidates.Count == 0)
            return Result.Ok(false);

        var picked =
            candidates.FirstOrDefault(c => c.Quality == task.Quality)
            ?? candidates.OrderByDescending(c => c.Quality).First();

        if (
            picked.PlexApiRatingKey == task.PlexApiRatingKey
            && picked.PlexApiMediaId == task.PlexApiMediaId
            && picked.PlexApiPartId == task.PlexApiPartId
        )
        {
            // Already current — no work to do.
            return Result.Ok(false);
        }

        var updatedRows = await dbContext
            .DownloadTaskTvShowEpisodeFile.Where(f => f.Id == taskId)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(f => f.PlexApiRatingKey, picked.PlexApiRatingKey)
                        .SetProperty(f => f.PlexApiMediaId, picked.PlexApiMediaId)
                        .SetProperty(f => f.PlexApiPartId, picked.PlexApiPartId)
                        .SetProperty(f => f.FileLocationUrl, picked.Key)
                        .SetProperty(f => f.DataReceived, 0L)
                        .SetProperty(f => f.DirectDownloadSnapshot, (DirectDownloadSnapshot?)null)
                        .SetProperty(f => f.Percentage, 0m)
                        .SetProperty(f => f.TimeRemaining, 0),
                ct
            );

        if (updatedRows == 0)
            return Result.Ok(false);

        _log.Here()
            .Information(
                "Refreshed metadata for episode task {FullTitle} on server {PlexServerId}: "
                    + "rating {OldRating}->{NewRating}, media {OldMedia}->{NewMedia}, part {OldPart}->{NewPart}",
                task.FullTitle,
                task.PlexServerId,
                task.PlexApiRatingKey,
                picked.PlexApiRatingKey,
                task.PlexApiMediaId,
                picked.PlexApiMediaId,
                task.PlexApiPartId,
                picked.PlexApiPartId
            );
        await _downloadQueue.CheckDownloadQueue([task.PlexServerId]);
        return Result.Ok(true);
    }

    private async Task<Result<bool>> RefreshMovieAsync(Guid taskId, CancellationToken ct)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        var task = await dbContext
            .DownloadTaskMovieFile.AsNoTracking()
            .Where(f => f.Id == taskId)
            .Select(f => new
            {
                f.Id,
                f.PlexServerId,
                f.PlexLibraryId,
                f.Quality,
                f.PlexApiRatingKey,
                f.PlexApiMediaId,
                f.PlexApiPartId,
                f.FileLocationUrl,
                f.FullTitle,
                MovieTitle = f.Parent!.Title,
                MovieYear = f.Parent!.Year,
            })
            .FirstOrDefaultAsync(ct);

        if (task is null)
            return Result.Ok(false);

        var candidates = await (
            from md in dbContext.PlexMovieData.AsNoTracking()
            join m in dbContext.PlexMovies on md.PlexMovieId equals m.Id
            where m.PlexServerId == task.PlexServerId
                && m.PlexLibraryId == task.PlexLibraryId
                && m.Title == task.MovieTitle
                && m.Year == task.MovieYear
            select new
            {
                md.PlexApiRatingKey,
                md.PlexApiMediaId,
                md.PlexApiPartId,
                md.Key,
                md.Quality,
            }
        ).ToListAsync(ct);

        if (candidates.Count == 0)
            return Result.Ok(false);

        var picked =
            candidates.FirstOrDefault(c => c.Quality == task.Quality)
            ?? candidates.OrderByDescending(c => c.Quality).First();

        if (
            picked.PlexApiRatingKey == task.PlexApiRatingKey
            && picked.PlexApiMediaId == task.PlexApiMediaId
            && picked.PlexApiPartId == task.PlexApiPartId
        )
        {
            return Result.Ok(false);
        }

        var updatedRows = await dbContext
            .DownloadTaskMovieFile.Where(f => f.Id == taskId)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(f => f.PlexApiRatingKey, picked.PlexApiRatingKey)
                        .SetProperty(f => f.PlexApiMediaId, picked.PlexApiMediaId)
                        .SetProperty(f => f.PlexApiPartId, picked.PlexApiPartId)
                        .SetProperty(f => f.FileLocationUrl, picked.Key)
                        .SetProperty(f => f.DataReceived, 0L)
                        .SetProperty(f => f.DirectDownloadSnapshot, (DirectDownloadSnapshot?)null)
                        .SetProperty(f => f.Percentage, 0m)
                        .SetProperty(f => f.TimeRemaining, 0),
                ct
            );

        if (updatedRows == 0)
            return Result.Ok(false);

        _log.Here()
            .Information(
                "Refreshed metadata for movie task {FullTitle} on server {PlexServerId}: "
                    + "rating {OldRating}->{NewRating}, media {OldMedia}->{NewMedia}, part {OldPart}->{NewPart}",
                task.FullTitle,
                task.PlexServerId,
                task.PlexApiRatingKey,
                picked.PlexApiRatingKey,
                task.PlexApiMediaId,
                picked.PlexApiMediaId,
                task.PlexApiPartId,
                picked.PlexApiPartId
            );
        await _downloadQueue.CheckDownloadQueue([task.PlexServerId]);
        return Result.Ok(true);
    }

    internal static (int Season, int Episode)? ParseSeasonEpisode(string fullTitle)
    {
        var combined = SeRegexCombined.Match(fullTitle);
        if (combined.Success)
            return (int.Parse(combined.Groups[1].Value), int.Parse(combined.Groups[2].Value));

        var xShort = SeRegexXShort.Match(fullTitle);
        if (xShort.Success)
            return (int.Parse(xShort.Groups[1].Value), int.Parse(xShort.Groups[2].Value));

        var seasonPath = SeasonPathRegex.Match(fullTitle);
        if (seasonPath.Success)
        {
            var parts = fullTitle.Split('/');
            var filename = parts.Length > 0 ? parts[^1] : fullTitle;
            var episode = EpisodeOnlyRegex.Match(filename);
            if (episode.Success)
                return (int.Parse(seasonPath.Groups[1].Value), int.Parse(episode.Groups[1].Value));
        }

        return null;
    }

}
