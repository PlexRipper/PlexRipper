using System.IO.Abstractions;
using FastEndpoints;
using FluentResults;
using FluentValidation;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Build;

internal sealed record DesktopLaunchBuildCommand(DesktopCommandSettings Settings) : ICommand<Result<int>>;

internal sealed class DesktopLaunchBuildCommandValidator : AbstractValidator<DesktopLaunchBuildCommand>
{
    public DesktopLaunchBuildCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.Settings).NotNull();
    }
}

internal sealed class DesktopLaunchBuildCommandHandler : ICommandHandler<DesktopLaunchBuildCommand, Result<int>>
{
    private readonly ILogger _log;
    private readonly BuildPaths _paths;
    private readonly IDesktopCommandRunner _commandRunner;
    private readonly IFileSystem _fileSystem;

    public DesktopLaunchBuildCommandHandler(
        ILogger log,
        BuildPaths paths,
        IDesktopCommandRunner commandRunner,
        IFileSystem fileSystem
    )
    {
        _log = log.ForContext<DesktopLaunchBuildCommandHandler>();
        _paths = paths;
        _commandRunner = commandRunner;
        _fileSystem = fileSystem;
    }

    public async Task<Result<int>> ExecuteAsync(DesktopLaunchBuildCommand command, CancellationToken cancellationToken)
    {
        var settings = command.Settings;
        var runtime = DesktopRuntimeCatalog.Get(settings.RuntimeIdentifier);

        var publishedExecutable = _fileSystem.Path.Combine(
            _paths.PublishDirectory(runtime.RuntimeIdentifier),
            runtime.MainExecutable
        );
        _log.Here()
            .Information(
                "Resolved published executable for {RuntimeIdentifier} to {PublishedExecutable} (launch mode: {LaunchMode})",
                runtime.RuntimeIdentifier,
                publishedExecutable,
                settings.LaunchMode
            );

        if (!_fileSystem.File.Exists(publishedExecutable))
            return Result.Fail<int>(
                $"Published executable for runtime '{runtime.RuntimeIdentifier}' was not found at '{publishedExecutable}'."
            );

        if (
            runtime.RuntimeIdentifier.StartsWith("win-", StringComparison.OrdinalIgnoreCase)
            && !OperatingSystem.IsWindows()
        )
        {
            if (await _commandRunner.CommandExistsAsync("wine"))
            {
                var wineExitCode = await _commandRunner.ExecuteCommandAsync("wine", [publishedExecutable]);
                _log.Here()
                    .Information(
                        "Wine launch for {RuntimeIdentifier} exited with code {ExitCode}",
                        runtime.RuntimeIdentifier,
                        wineExitCode
                    );
                return Result.Ok(wineExitCode);
            }

            _log.Here().Warning("Wine is not available on this host, so the Windows build was not launched.");
            return Result.Ok(0);
        }

        var exitCode = await _commandRunner.ExecuteCommandAsync(publishedExecutable, []);
        _log.Here()
            .Information(
                "Published executable for {RuntimeIdentifier} exited with code {ExitCode}",
                runtime.RuntimeIdentifier,
                exitCode
            );
        return Result.Ok(exitCode);
    }
}
