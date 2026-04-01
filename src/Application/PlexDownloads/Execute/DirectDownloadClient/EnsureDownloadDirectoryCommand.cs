using System.IO.Abstractions;

namespace Reaparr.Application;

public record EnsureDownloadDirectoryCommand(string Directory, long FileSize) : ICommand<Result>;

public class EnsureDownloadDirectoryCommandValidator : AbstractValidator<EnsureDownloadDirectoryCommand>
{
    public EnsureDownloadDirectoryCommandValidator()
    {
        RuleFor(x => x.Directory).NotEmpty().WithMessage("Directory cannot be empty.");
        RuleFor(x => x.FileSize).GreaterThan(0).WithMessage("File size must be greater than zero.");
    }
}

public class EnsureDownloadDirectoryCommandHandler : ICommandHandler<EnsureDownloadDirectoryCommand, Result>
{
    private readonly IPath _path;
    private readonly IDirectory _directory;

    public EnsureDownloadDirectoryCommandHandler(IPath path, IDirectory directory)
    {
        _path = path;
        _directory = directory;
    }

    public async Task<Result> ExecuteAsync(EnsureDownloadDirectoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await Task.CompletedTask;

            var directory = command.Directory;
            var fileSize = command.FileSize;

            var createDirectoryResult = Result.Try(() => _directory.CreateDirectory(directory));
            if (createDirectoryResult.IsFailed)
                return createDirectoryResult.ToResult();

            var availableSpace = _path.GetAvailableSpaceByDirectory(directory);
            if (availableSpace.IsFailed)
                return availableSpace.ToResult();

            if (availableSpace.Value < fileSize)
                return Result.Fail($"There is not enough space available in root directory {directory}").LogError();

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
