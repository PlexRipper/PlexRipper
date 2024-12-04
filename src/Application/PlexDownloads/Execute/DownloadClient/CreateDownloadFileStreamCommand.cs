using System.IO.Abstractions;
using FileSystem.Contracts;
using FluentValidation;
using Logging.Interface;

public record CreateDownloadFileStreamCommand(string Directory, string FileName, long FileSize)
    : IRequest<Result<Stream>>;

public class CreateDownloadFileStreamCommandValidator : AbstractValidator<CreateDownloadFileStreamCommand>
{
    public CreateDownloadFileStreamCommandValidator()
    {
        RuleFor(x => x.Directory).NotEmpty().WithMessage("Directory cannot be empty.");
        RuleFor(x => x.FileName).NotEmpty().WithMessage("File name cannot be empty.");
        RuleFor(x => x.FileSize).GreaterThan(0).WithMessage("File size must be greater than zero.");
    }
}

public class CreateDownloadFileStreamCommandHandler : IRequestHandler<CreateDownloadFileStreamCommand, Result<Stream>>
{
    private readonly IFileSystem _abstractedFileSystem;
    private readonly ILog _log;

    public CreateDownloadFileStreamCommandHandler(IFileSystem abstractedFileSystem, ILog log)
    {
        _abstractedFileSystem = abstractedFileSystem;
        _log = log;
    }

    public async Task<Result<Stream>> Handle(
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

            var createDirectoryResult = Result.Try(() => _abstractedFileSystem.Directory.CreateDirectory(directory));
            if (createDirectoryResult.IsFailed)
                return createDirectoryResult.ToResult();

            // TODO:This might need to be determined sooner, like when adding downloadTasks
            var availableSpace = _abstractedFileSystem.GetAvailableSpaceByDirectory(directory);
            if (availableSpace.IsFailed)
                return availableSpace.ToResult();

            if (availableSpace.Value < fileSize)
                return Result.Fail($"There is not enough space available in root directory {directory}").LogError();

            var combineResult = Result.Try((() => _abstractedFileSystem.Path.Combine(directory, fileName)));
            if (combineResult.IsFailed)
                return combineResult.ToResult();

            var filePath = combineResult.Value;
            Stream fileStream;
            if (_abstractedFileSystem.File.Exists(filePath))
            {
                var openResult = Result.Try(
                    () =>
                        _abstractedFileSystem.File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Delete)
                );
                if (openResult.IsFailed)
                    return openResult.ToResult().LogError();

                fileStream = openResult.Value;
            }
            else
            {
                var createResult = Result.Try(
                    () => _abstractedFileSystem.File.Create(filePath, 2048, FileOptions.Asynchronous)
                );
                if (createResult.IsFailed)
                    return createResult.ToResult().LogError();

                fileStream = createResult.Value;
            }

            // Pre-allocate the required file size
            fileStream.SetLength(fileSize);
            return Result.Ok(fileStream);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
