using CliWrap;
using CliWrap.EventStream;
using CliWrap.Exceptions;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Reaparr.Build;

internal interface IDesktopCommandRunner
{
    Task RunCommandAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory = null);

    Task<int> ExecuteCommandAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory = null);

    Task RequireCommandAsync(string command);

    Task<bool> CommandExistsAsync(string command);
}

internal sealed class DesktopCommandRunner(
    BuildPaths paths,
    DesktopCommandSettings settings,
    IFileSystem fileSystem,
    ILogger logger
) : IDesktopCommandRunner
{
    public async Task RunCommandAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory = null)
    {
        LogCommand(fileName, arguments);
        if (settings.DryRun)
        {
            return;
        }

        var resolvedWorkingDirectory = workingDirectory ?? paths.RootDirectory;
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

        var resolvedWorkingDirectory = workingDirectory ?? paths.RootDirectory;
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

    public Task<bool> CommandExistsAsync(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return Task.FromResult(false);
        }

        if (
            fileSystem.Path.IsPathRooted(command)
            || command.Contains(fileSystem.Path.DirectorySeparatorChar)
            || command.Contains(fileSystem.Path.AltDirectorySeparatorChar)
        )
        {
            return Task.FromResult(fileSystem.File.Exists(command));
        }

        var pathValue = System.Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return Task.FromResult(false);
        }

        var candidates = new List<string> { command };

        if (OperatingSystem.IsWindows())
        {
            var pathExtValue = System.Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT;.COM";
            var pathExts = pathExtValue
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!fileSystem.Path.HasExtension(command))
            {
                foreach (var ext in pathExts)
                {
                    candidates.Add(command + ext);
                }
            }
        }

        foreach (
            var directory in pathValue.Split(
                fileSystem.Path.PathSeparator,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            )
        )
        {
            foreach (var candidate in candidates)
            {
                var fullPath = fileSystem.Path.Combine(directory, candidate);
                if (fileSystem.File.Exists(fullPath))
                {
                    return Task.FromResult(true);
                }
            }
        }

        return Task.FromResult(false);
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
