using System.IO.Abstractions;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public class DeleteDownloadTaskFilesCommandValidator : AbstractValidator<DeleteDownloadTaskFilesCommand>
{
    public DeleteDownloadTaskFilesCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.Keys).NotEmpty();
    }
}

public class DeleteDownloadTaskFilesCommandHandler : ICommandHandler<DeleteDownloadTaskFilesCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IFile _file;
    private readonly IDirectory _directory;
    private readonly IPath _path;

    public DeleteDownloadTaskFilesCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IFile file,
        IDirectory directory,
        IPath path
    )
    {
        _log = log.ForContext<DeleteDownloadTaskFilesCommandHandler>();
        _dbContext = dbContext;
        _file = file;
        _directory = directory;
        _path = path;
    }

    public async Task<Result> ExecuteAsync(DeleteDownloadTaskFilesCommand command, CancellationToken cancellationToken)
    {
        var keys = command.Keys;

        var movieFileIds = keys.Where(k => k.Type is DownloadTaskType.MovieData or DownloadTaskType.MoviePart)
            .Select(k => k.Id)
            .ToList();

        var episodeFileIds = keys.Where(k => k.Type is DownloadTaskType.EpisodeData or DownloadTaskType.EpisodePart)
            .Select(k => k.Id)
            .ToList();

        var movieFileTask = _dbContext
            .DownloadTaskMovieFile.AsNoTracking()
            .Where(x => movieFileIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var episodeFileTask = _dbContext
            .DownloadTaskTvShowEpisodeFile.AsNoTracking()
            .Where(x => episodeFileIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        await Task.WhenAll(movieFileTask, episodeFileTask);

        var allFileTasks = movieFileTask.Result.Cast<DownloadTaskFileBase>().Concat(episodeFileTask.Result).ToList();

        foreach (var task in allFileTasks)
            DeleteFileIfPresent(task);

        // After deleting files, try to clean up any empty directories left behind.
        // Process unique download directories so we don't attempt to clean the same folder twice.
        var downloadDirectories = allFileTasks
            .Select(x => x.DownloadDirectory)
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var directory in downloadDirectories)
            DeleteDirectoryIfEmpty(directory);

        return Result.Ok();
    }

    /// <summary>
    /// Deletes the source file for the given task from the download directory.
    /// Checks both the <c>.reaptemp</c>-suffixed path (active download) and the plain path
    /// (file already renamed after keep-in-downloads or completed move-to-same-folder step).
    /// </summary>
    private void DeleteFileIfPresent(DownloadTaskFileBase task)
    {
        // DownloadFilePath includes the .reaptemp suffix — check it first.
        var reapTempPath = task.DownloadFilePath;
        var plainPath = reapTempPath.RemoveReapTempSuffix();

        foreach (var candidate in new[] { reapTempPath, plainPath })
        {
            if (string.IsNullOrEmpty(candidate) || !_file.Exists(candidate))
                continue;

            _log.Here()
                .Debug("Deleting download file for {DownloadTaskTitle} at {FilePath}", task.FullTitle, candidate);

            Result.Try(() => _file.Delete(candidate)).LogIfFailed();
            return;
        }

        _log.Here()
            .Debug(
                "No download file found to delete for {DownloadTaskTitle} (checked {ReapTempPath} and {PlainPath})",
                task.FullTitle,
                reapTempPath,
                plainPath
            );
    }

    /// <summary>
    /// Recursively deletes a directory and its parents as long as each one is empty.
    /// </summary>
    private void DeleteDirectoryIfEmpty(string directory)
    {
        if (string.IsNullOrEmpty(directory) || !_directory.Exists(directory))
            return;

        var entries = Result.Try(() => _directory.GetFileSystemEntries(directory).ToList());
        if (entries.IsFailed)
        {
            entries.LogIfFailed();
            return;
        }

        if (entries.Value.Count > 0)
            return;

        _log.Here().Debug("Deleting empty download directory {Directory}", directory);
        var deleteResult = Result.Try(() => _directory.Delete(directory));
        if (deleteResult.IsFailed)
        {
            deleteResult.LogIfFailed();
            return;
        }

        // Walk up one level and try again (e.g. remove Season folder, then TvShow folder).
        var parent = Result.Try(() => _path.GetDirectoryName(directory));
        if (parent.IsSuccess && !string.IsNullOrEmpty(parent.Value))
            DeleteDirectoryIfEmpty(parent.Value);
    }
}
