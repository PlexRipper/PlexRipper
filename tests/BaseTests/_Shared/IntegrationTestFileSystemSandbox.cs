namespace Reaparr.BaseTests;

public static class IntegrationTestFileSystemSandbox
{
    private static ILogger? _log;
    private const string SANDBOX_FOLDER = ".test-artifacts";
    private const string INTEGRATION_SANDBOX_FOLDER = "integration-fs";

    public static string Create(string memoryDbName, ILogger log)
    {
        _log = log.ForContext(typeof(IntegrationTestFileSystemSandbox));

        try
        {
            var projectRoot = GetProjectRoot();

            var sandboxPath = Path.Combine(projectRoot, SANDBOX_FOLDER, INTEGRATION_SANDBOX_FOLDER, memoryDbName);
            Directory.CreateDirectory(sandboxPath);
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
