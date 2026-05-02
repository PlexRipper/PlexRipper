using System.IO.Abstractions;

namespace Reaparr.Build;

internal sealed class FileSystemTasks(IFileSystem fileSystem)
{
    public void CopyDirectory(string source, string target)
    {
        EnsureCopyTargetIsNotNestedInsideSource(source, target);
        fileSystem.Directory.CreateDirectory(target);

        foreach (var directory in fileSystem.Directory.EnumerateDirectories(source))
        {
            CopyDirectory(directory, fileSystem.Path.Combine(target, fileSystem.Path.GetFileName(directory)));
        }

        foreach (var file in fileSystem.Directory.EnumerateFiles(source))
        {
            fileSystem.File.Copy(file, fileSystem.Path.Combine(target, fileSystem.Path.GetFileName(file)), overwrite: true);
        }
    }

    public void ClearDirectory(string directory)
    {
        fileSystem.Directory.CreateDirectory(directory);

        foreach (var file in fileSystem.Directory.EnumerateFiles(directory))
        {
            fileSystem.File.Delete(file);
        }

        foreach (var childDirectory in fileSystem.Directory.EnumerateDirectories(directory))
        {
            fileSystem.Directory.Delete(childDirectory, recursive: true);
        }
    }

    public void ClearArtifactDirectory(string repositoryRoot, string artifactDirectory)
    {
        EnsureSafeArtifactClearTarget(repositoryRoot, artifactDirectory);
        ClearDirectory(artifactDirectory);
    }

    private void EnsureCopyTargetIsNotNestedInsideSource(string source, string target)
    {
        var sourcePath = NormalizeDirectoryPath(source);
        var targetPath = NormalizeDirectoryPath(target);

        if (targetPath.StartsWith(sourcePath + fileSystem.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Refusing to copy '{sourcePath}' into nested target '{targetPath}'."
            );
        }
    }

    private void EnsureSafeArtifactClearTarget(string repositoryRoot, string artifactDirectory)
    {
        var rootPath = NormalizeDirectoryPath(repositoryRoot);
        var artifactPath = NormalizeDirectoryPath(artifactDirectory);

        if (string.Equals(rootPath, artifactPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Refusing to clear the repository root as an artifact directory.");
        }

        var defaultArtifactsRoot = NormalizeDirectoryPath(fileSystem.Path.Combine(rootPath, ".artifacts"));
        if (!artifactPath.StartsWith(defaultArtifactsRoot + fileSystem.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Refusing to clear custom artifact directory '{artifactPath}'. Choose a path under '{defaultArtifactsRoot}' or use --preserve-existing-artifacts."
            );
        }
    }

    private string NormalizeDirectoryPath(string path) =>
        fileSystem.Path.GetFullPath(path).TrimEnd(fileSystem.Path.DirectorySeparatorChar, fileSystem.Path.AltDirectorySeparatorChar);
}
