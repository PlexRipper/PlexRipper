using System.IO.Abstractions;
using FastEndpoints;
using FluentValidation;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application;

public record CreateDownloadFileStreamCommand(string Directory, string FileName, long FileSize)
    : ICommand<Result<Stream>>;

public class CreateDownloadFileStreamCommandValidator : AbstractValidator<CreateDownloadFileStreamCommand>
{
    public CreateDownloadFileStreamCommandValidator()
    {
        RuleFor(x => x.Directory).NotEmpty().WithMessage("Directory cannot be empty.");
        RuleFor(x => x.FileName).NotEmpty().WithMessage("File name cannot be empty.");
        RuleFor(x => x.FileSize).GreaterThan(0).WithMessage("File size must be greater than zero.");
    }
}

public class CreateDownloadFileStreamCommandHandler : ICommandHandler<CreateDownloadFileStreamCommand, Result<Stream>>
{
    private readonly IPath _path;
    private readonly IDirectory _directory;
    private readonly IFile _file;

    public CreateDownloadFileStreamCommandHandler(IPath path, IDirectory directory, IFile file)
    {
        _path = path;
        _directory = directory;
        _file = file;
    }

    public async Task<Result<Stream>> ExecuteAsync(
        CreateDownloadFileStreamCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await Task.CompletedTask;

            var directory = command.Directory;
            var fileName = command.FileName;
            var fileSize = command.FileSize;

            var createDirectoryResult = Result.Try(() => _directory.CreateDirectory(directory));
            if (createDirectoryResult.IsFailed)
                return createDirectoryResult.ToResult();

            var availableSpace = _path.GetAvailableSpaceByDirectory(directory);
            if (availableSpace.IsFailed)
                return availableSpace.ToResult();

            if (availableSpace.Value < fileSize)
                return Result.Fail($"There is not enough space available in root directory {directory}").LogError();

            var combineResult = Result.Try((() => _path.Combine(directory, fileName)));
            if (combineResult.IsFailed)
                return combineResult.ToResult();

            var filePath = combineResult.Value;
            var openOrCreateResult = Result.Try(() =>
                _file.Open(
                    filePath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite | FileShare.Delete
                )
            );
            if (openOrCreateResult.IsFailed)
                return openOrCreateResult.ToResult().LogError();

            var fileStream = openOrCreateResult.Value;

            // Ensure file length is at least the requested size without truncating existing data
            if (fileStream.Length < fileSize)
                fileStream.SetLength(fileSize);

            return Result.Ok((Stream)fileStream);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
