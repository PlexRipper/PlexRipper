using CliWrap;
using CliWrap.Buffered;
using CliWrap.EventStream;
using CliWrap.Exceptions;
using Microsoft.Extensions.Logging;

namespace Reaparr.Build;

internal sealed class DesktopCommandRunner(BuildPaths paths, DesktopCommandSettings settings, ILogger logger)
{
    public async Task RunCommandAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory = null)
    {
        LogCommand(fileName, arguments);
        if (settings.DryRun)
        {
            return;
        }

        var resolvedWorkingDirectory = workingDirectory ?? paths.RootDirectory.FullName;
        logger.LogInformation("Executing command in {WorkingDirectory}", resolvedWorkingDirectory);

        try
        {
            var command = Cli.Wrap(fileName)
                .WithArguments(arguments)
                .WithWorkingDirectory(resolvedWorkingDirectory);

            await foreach (var commandEvent in command.ListenAsync())
            {
                switch (commandEvent)
                {
                    case StandardOutputCommandEvent stdOut when !string.IsNullOrWhiteSpace(stdOut.Text):
                        logger.LogDebug("{Output}", stdOut.Text);
                        break;
                    case StandardErrorCommandEvent stdErr when !string.IsNullOrWhiteSpace(stdErr.Text):
                        logger.LogWarning("{Output}", stdErr.Text);
                        break;
                }
            }
        }
        catch (CommandExecutionException ex)
        {
            throw new InvalidOperationException(
                $"Command '{fileName}' failed with exit code {ex.ExitCode} in working directory '{resolvedWorkingDirectory}'. Full command: {FormatCommand(fileName, arguments)}",
                ex
            );
        }
    }

    public async Task<int> ExecuteCommandAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null
    )
    {
        LogCommand(fileName, arguments);
        if (settings.DryRun)
        {
            return 0;
        }

        var resolvedWorkingDirectory = workingDirectory ?? paths.RootDirectory.FullName;
        logger.LogInformation("Executing command in {WorkingDirectory}", resolvedWorkingDirectory);

        var result = await Cli.Wrap(fileName)
            .WithArguments(arguments)
            .WithWorkingDirectory(resolvedWorkingDirectory)
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        logger.LogInformation(
            "Command exited with code {ExitCode}: {Command}",
            result.ExitCode,
            FormatCommand(fileName, arguments)
        );

        return result.ExitCode;
    }

    public async Task RequireCommandAsync(string command)
    {
        if (!await CommandExistsAsync(command))
        {
            throw new FileNotFoundException(
                $"Required command '{command}' was not found on PATH. Install it or adjust your environment before running desktop builds.",
                command
            );
        }
    }

    public async Task<bool> CommandExistsAsync(string command)
    {
        var executable = OperatingSystem.IsWindows() ? "where" : "which";
        var result = await Cli.Wrap(executable)
            .WithArguments([command])
            .WithWorkingDirectory(paths.RootDirectory.FullName)
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        return result.ExitCode is 0;
    }

    private void LogCommand(string fileName, IReadOnlyList<string> arguments)
    {
        logger.LogInformation("> {Command}", FormatCommand(fileName, arguments));
    }

    private static string FormatCommand(string fileName, IReadOnlyList<string> arguments) =>
        string.Join(' ', new[] { fileName }.Concat(arguments.Select(QuoteIfNeeded)));

    private static string QuoteIfNeeded(string value) =>
        value.Any(char.IsWhiteSpace) ? $"\"{value.Replace("\"", "\\\"")}\"" : value;
}
