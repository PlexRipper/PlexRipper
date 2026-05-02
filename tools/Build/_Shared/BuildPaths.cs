using System.IO.Abstractions;

namespace Reaparr.Build;

internal sealed class BuildPaths(string rootDirectory, IFileSystem fileSystem)
{
    public string RootDirectory { get; } = rootDirectory;

    public string AppHostProject => fileSystem.Path.Combine(RootDirectory, "src", "AppHost", "AppHost.csproj");

    public string ClientAppDirectory => fileSystem.Path.Combine(RootDirectory, "src", "AppHost", "ClientApp");

    public string FrontendPublicDirectory => fileSystem.Path.Combine(ClientAppDirectory, ".output", "public");

    public string PublishDirectory(string rid) =>
        fileSystem.Path.Combine(RootDirectory, "src", "AppHost", "bin", "Publish", "Desktop", rid);

    public string ArtifactDirectory(string rid) => fileSystem.Path.Combine(RootDirectory, ".artifacts", rid);

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
