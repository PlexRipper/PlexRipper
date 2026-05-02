namespace Reaparr.Build;

internal sealed record BuildPaths(DirectoryInfo RootDirectory)
{
    public string AppHostProject => Path.Combine(RootDirectory.FullName, "src", "AppHost", "AppHost.csproj");

    public string ClientAppDirectory => Path.Combine(RootDirectory.FullName, "src", "AppHost", "ClientApp");

    public string FrontendPublicDirectory => Path.Combine(ClientAppDirectory, ".output", "public");

    public string PublishDirectory(string rid) =>
        Path.Combine(RootDirectory.FullName, "src", "AppHost", "bin", "Publish", "Desktop", rid);

    public string ArtifactDirectory(string rid) => Path.Combine(RootDirectory.FullName, ".artifacts", rid);

    public static BuildPaths FromCurrentDirectory()
    {
        var directory = new DirectoryInfo(System.Environment.CurrentDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Reaparr.sln")))
            {
                return new BuildPaths(directory);
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Reaparr.sln from the current directory.");
    }
}
