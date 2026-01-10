using System.IO.Abstractions;
using FastEndpoints;
using FluentValidation;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record CleanUpDownloadTaskFoldersCommand(DownloadTaskKey DownloadTaskKey) : ICommand<Result>;

public class CleanUpDownloadTaskFoldersValidator : AbstractValidator<CleanUpDownloadTaskFoldersCommand>
{
    public CleanUpDownloadTaskFoldersValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class CleanUpDownloadTaskFoldersHandler : ICommandHandler<CleanUpDownloadTaskFoldersCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IPath _path;
    private readonly IDirectory _directory;

    public CleanUpDownloadTaskFoldersHandler(ILogger log, IReaparrDbContext dbContext, IPath path, IDirectory directory)
    {
        _log = log.ForContext<CleanUpDownloadTaskFoldersHandler>();
        _dbContext = dbContext;
        _path = path;
        _directory = directory;
    }

    public async Task<Result> ExecuteAsync(
        CleanUpDownloadTaskFoldersCommand command,
        CancellationToken cancellationToken
    )
    {
        var downloadTaskKey = command.DownloadTaskKey;
        var downloadTask = await _dbContext.GetDownloadTaskFileAsync(downloadTaskKey, cancellationToken);

        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.Id).LogError();

        var filePath = downloadTask.DownloadFilePath;
        if (string.IsNullOrEmpty(filePath))
            return ResultExtensions.IsEmpty(nameof(filePath)).LogError();

        // This deletes the Season or movie folder
        var result = DeleteDirectoryFromFilePath(filePath);
        if (result.IsFailed)
            return result;

        if (downloadTask.DownloadTaskType == DownloadTaskType.EpisodeData)
        {
            // This deletes the TvShow folder
            var result2 = DeleteDirectoryFromFilePath(downloadTask.DownloadDirectory);
            if (result2.IsFailed)
                return result2;
        }

        return Result.Ok();
    }

    private Result DeleteDirectoryFromFilePath(string filePath)
    {
        var directoryNameResult = Result.Try(() => _path.GetDirectoryName(filePath));

        if (directoryNameResult.IsFailed || string.IsNullOrEmpty(directoryNameResult.Value))
            return ResultExtensions.IsEmpty(nameof(directoryNameResult.Value)).LogError();

        var parentDirectory = directoryNameResult.Value;

        if (!_directory.Exists(parentDirectory))
            return Result.Ok();

        var entriesResult = Result.Try(() => _directory.GetFileSystemEntries(parentDirectory).ToList());
        if (entriesResult.IsFailed)
        {
            return entriesResult.ToResult().LogError();
        }

        if (!entriesResult.Value.Any())
        {
            _directory.Delete(parentDirectory);
            return Result.Ok();
        }

        var entries = string.Join(", ", entriesResult.Value.Select(entry => _path.GetFileName(entry)));
        _log.Here()
            .Debug(
                "Skipping delete for {DirectoryPath} because it still contains: {DirectoryEntries}",
                parentDirectory,
                entries
            );

        return Result.Ok();
    }
}
