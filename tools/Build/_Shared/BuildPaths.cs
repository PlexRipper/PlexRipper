using System.IO.Abstractions;

namespace Reaparr.Build;

internal sealed class BuildPaths
{
    private readonly IFileSystem _fileSystem;

    public BuildPaths(string rootDirectory, IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        RootDirectory = rootDirectory;
    }

    public string RootDirectory { get; }

    public string AppHostProject => _fileSystem.Path.Combine(RootDirectory, "src", "AppHost", "AppHost.csproj");

    public string ClientAppDirectory => _fileSystem.Path.Combine(RootDirectory, "src", "AppHost", "ClientApp");

    public string FrontendPublicDirectory => _fileSystem.Path.Combine(ClientAppDirectory, ".output", "public");

    public string PublishDirectory(string rid) => _fileSystem.Path.Combine(ArtifactDirectory(rid), "publish");

    public string ArtifactDirectory(string rid) => _fileSystem.Path.Combine(RootDirectory, ".artifacts", rid);

    public static BuildPaths FromCurrentDirectory(IFileSystem fileSystem)
    {
        var directory = fileSystem.DirectoryInfo.New(System.Environment.CurrentDirectory);
        while (directory is not null)
        {
            if (fileSystem.File.Exists(fileSystem.Path.Combine(directory.FullName, "Reaparr.sln")))
            {
                return new BuildPaths(directory.FullName, fileSystem);
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Reaparr.sln from the current directory.");
    }
}
