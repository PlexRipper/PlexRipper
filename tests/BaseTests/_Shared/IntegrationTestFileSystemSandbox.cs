namespace Reaparr.BaseTests;

public static class IntegrationTestFileSystemSandbox
{
    private static ILogger? _log;
    private const string SANDBOX_FOLDER = ".test-artifacts";
    private const string INTEGRATION_SANDBOX_FOLDER = "integration-fs";

    public static string GetSandboxFolder(string memoryDbName) =>
        Path.Combine(GetProjectRoot(), SANDBOX_FOLDER, INTEGRATION_SANDBOX_FOLDER, memoryDbName);

    public static string Create(string memoryDbName, ILogger log)
    {
        _log = log.ForContext(typeof(IntegrationTestFileSystemSandbox));

        try
        {
            var sandboxPath = Path.GetFullPath(GetSandboxFolder(memoryDbName));
            Directory.CreateDirectory(sandboxPath);
            IPathProvider pathProvider = new PathProvider();

            var pathsToCreate = new[]
            {
                pathProvider.ConfigDirectory,
                pathProvider.DefaultDownloadsDestinationFolder,
                pathProvider.DefaultMovieDestinationFolder,
                pathProvider.DefaultTvShowsDestinationFolder,
                pathProvider.DefaultMusicDestinationFolder,
                pathProvider.DefaultPhotosDestinationFolder,
                pathProvider.DefaultOtherDestinationFolder,
                pathProvider.DefaultGamesDestinationFolder,
            }.Select(Path.GetFullPath);

            foreach (var path in pathsToCreate)
            {
                if (!path.StartsWith(sandboxPath, StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        $"Refusing to create integration test directory outside sandbox. Path: '{path}', Sandbox: '{sandboxPath}'."
                    );

                Directory.CreateDirectory(path);
            }

            _log.Here()
                .Information(
                    "Created integration test filesystem sandbox directory for {DatabaseName}: {SandboxPath}",
                    memoryDbName,
                    sandboxPath
                );
            return sandboxPath;
        }
        catch (Exception ex)
        {
            _log.Here()
                .Error(ex, "Failed to create integration test filesystem sandbox for {DatabaseName}", memoryDbName);
            throw;
        }
    }

    private static string GetProjectRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var hasSolutionFile = File.Exists(Path.Combine(current.FullName, "Reaparr.sln"));
            var hasGitDirectory = Directory.Exists(Path.Combine(current.FullName, ".git"));
            if (hasSolutionFile || hasGitDirectory)
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate project root for integration test filesystem sandbox.");
    }
}
