using Autofac;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Abstractions;

namespace Reaparr.Build.UnitTests;

public class DesktopBuildWorkflowUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldRequireVpkBeforePublishing_WhenPackaging()
    {
        // Arrange
        const string root = "/repo";
        SetupFileSystem(system =>
        {
            system.AddFile("/repo/Reaparr.sln", string.Empty);
            system.AddDirectory("/repo/src/AppHost/ClientApp/.output/public");
        });

        var fileSystem = Mock.Container.Resolve<IFileSystem>();
        var paths = new BuildPaths(root, fileSystem);
        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            Version = "1.2.3",
            InformationalVersion = "1.2.3-dev.1",
            SkipRestore = true,
        };
        var commandRunner = new RecordingDesktopCommandRunner();
        var workflow = DesktopBuildWorkflow.Create(
            paths,
            settings,
            NullLoggerFactory.Instance,
            fileSystem,
            commandRunner
        );

        // Act
        await workflow.PackageAsync();

        // Assert
        commandRunner.Commands.Select(x => x.Operation).ShouldBe(["require", "require", "run", "run"]);
        commandRunner.Commands[0].FileName.ShouldBe("vpk");
        commandRunner.Commands[1].FileName.ShouldBe("dotnet");
        commandRunner.Commands[2].FileName.ShouldBe("dotnet");
        commandRunner.Commands[2].Arguments.ShouldContain("publish");
        commandRunner.Commands[3].FileName.ShouldBe("vpk");
    }

    [Test]
    public async Task ShouldNotRequireVpk_WhenRunSkipsPackaging()
    {
        // Arrange
        const string root = "/repo";
        SetupFileSystem(system =>
        {
            system.AddFile("/repo/Reaparr.sln", string.Empty);
            system.AddDirectory("/repo/src/AppHost/ClientApp/.output/public");
        });

        var fileSystem = Mock.Container.Resolve<IFileSystem>();
        var paths = new BuildPaths(root, fileSystem);
        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            Version = "1.2.3",
            InformationalVersion = "1.2.3-dev.1",
            SkipPackage = true,
            SkipRestore = true,
            DryRun = true,
        };
        var commandRunner = new RecordingDesktopCommandRunner();
        var workflow = DesktopBuildWorkflow.Create(
            paths,
            settings,
            NullLoggerFactory.Instance,
            fileSystem,
            commandRunner
        );

        // Act
        var exitCode = await workflow.RunAsync();

        // Assert
        exitCode.ShouldBe(0);
        commandRunner.Commands.ShouldNotContain(x => x.Operation == "require" && x.FileName == "vpk");
    }

    private sealed class RecordingDesktopCommandRunner : IDesktopCommandRunner
    {
        public List<RecordedCommand> Commands { get; } = [];

        public Task RunCommandAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory = null)
        {
            Commands.Add(new RecordedCommand("run", fileName, arguments));
            return Task.CompletedTask;
        }

        public Task<int> ExecuteCommandAsync(
            string fileName,
            IReadOnlyList<string> arguments,
            string? workingDirectory = null
        )
        {
            Commands.Add(new RecordedCommand("execute", fileName, arguments));
            return Task.FromResult(0);
        }

        public Task RequireCommandAsync(string command)
        {
            Commands.Add(new RecordedCommand("require", command, []));
            return Task.CompletedTask;
        }

        public Task<bool> CommandExistsAsync(string command) => Task.FromResult(true);
    }

    private sealed record RecordedCommand(
        string Operation,
        string FileName,
        IReadOnlyList<string> Arguments
    );
}
