using Autofac;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Reaparr.Build.UnitTests;

public class DesktopPublishWorkflowUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldReuseExistingFrontendOutput_WhenFrontendPublicDirectoryAlreadyExists()
    {
        // Arrange
        const string root = "/repo";
        SetupFileSystem(system =>
        {
            system.AddDirectory("/repo/src/AppHost/ClientApp/.output/public");
            system.AddFile("/repo/src/AppHost/ClientApp/.output/public/index.html", new MockFileData("frontend"));
        });

        var fileSystem = Mock.Container.Resolve<IFileSystem>();
        var paths = new BuildPaths(root, fileSystem);
        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            Version = "1.2.3",
            InformationalVersion = "1.2.3-dev.1",
            DryRun = true,
            SkipRestore = true,
        };
        var commandRunner = new RecordingDesktopCommandRunner();

        var sut = CreateSut(paths, settings, commandRunner, fileSystem);

        // Act
        await sut.PublishAsync();

        // Assert
        commandRunner.Commands.ShouldNotContain(x => x.FileName == "bun");
        commandRunner.Commands.Count(x => x.FileName == "dotnet").ShouldBe(1);
    }

    [Test]
    public async Task ShouldClearStaleWwwrootFiles_WhenCopyingFrontendOutput()
    {
        // Arrange
        const string root = "/repo";
        SetupFileSystem(system =>
        {
            system.AddDirectory("/repo/src/AppHost/ClientApp/.output/public");
            system.AddFile("/repo/src/AppHost/ClientApp/.output/public/index.html", new MockFileData("frontend"));
            system.AddDirectory("/repo/src/AppHost/ClientApp/.output/public/assets");
            system.AddFile("/repo/src/AppHost/ClientApp/.output/public/assets/app.js", new MockFileData("app"));
            system.AddDirectory("/repo/.artifacts/linux-x64/publish/wwwroot");
            system.AddFile("/repo/.artifacts/linux-x64/publish/wwwroot/stale.html", new MockFileData("stale"));
            system.AddDirectory("/repo/.artifacts/linux-x64/publish/wwwroot/old-assets");
            system.AddFile("/repo/.artifacts/linux-x64/publish/wwwroot/old-assets/old.js", new MockFileData("old"));
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
        var sut = CreateSut(paths, settings, commandRunner, fileSystem);

        // Act
        await sut.PublishAsync();

        // Assert
        fileSystem.File.Exists("/repo/.artifacts/linux-x64/publish/wwwroot/index.html").ShouldBeTrue();
        fileSystem.File.Exists("/repo/.artifacts/linux-x64/publish/wwwroot/assets/app.js").ShouldBeTrue();
        fileSystem.File.Exists("/repo/.artifacts/linux-x64/publish/wwwroot/stale.html").ShouldBeFalse();
        fileSystem.Directory.Exists("/repo/.artifacts/linux-x64/publish/wwwroot/old-assets").ShouldBeFalse();
    }

    private static DesktopPublishWorkflow CreateSut(
        BuildPaths paths,
        DesktopCommandSettings settings,
        IDesktopCommandRunner commandRunner,
        IFileSystem fileSystem
    ) =>
        new(
            paths,
            DesktopRuntimeCatalog.Get(settings.RuntimeIdentifier),
            settings,
            commandRunner,
            new FileSystemTasks(fileSystem),
            fileSystem,
            NullLogger<DesktopPublishWorkflow>.Instance
        );

    private sealed class RecordingDesktopCommandRunner : IDesktopCommandRunner
    {
        public List<RecordedCommand> Commands { get; } = [];

        public Task RunCommandAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory = null)
        {
            Commands.Add(new RecordedCommand(fileName));
            return Task.CompletedTask;
        }

        public Task<int> ExecuteCommandAsync(
            string fileName,
            IReadOnlyList<string> arguments,
            string? workingDirectory = null
        ) => Task.FromResult(0);

        public Task RequireCommandAsync(string command) => Task.CompletedTask;

        public Task<bool> CommandExistsAsync(string command) => Task.FromResult(true);
    }

    private sealed record RecordedCommand(string FileName);
}
