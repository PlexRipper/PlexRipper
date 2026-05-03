using System.IO.Abstractions;

namespace Reaparr.Build;

internal sealed class FileSystemTasks
{
    private readonly IFileSystem _fileSystem;

    public FileSystemTasks(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void CopyDirectory(string source, string target)
    {
        EnsureCopyTargetIsNotNestedInsideSource(source, target);
        _fileSystem.Directory.CreateDirectory(target);

        foreach (var directory in _fileSystem.Directory.EnumerateDirectories(source))
        {
            CopyDirectory(directory, _fileSystem.Path.Combine(target, _fileSystem.Path.GetFileName(directory)));
        }

        foreach (var file in _fileSystem.Directory.EnumerateFiles(source))
        {
            _fileSystem.File.Copy(
                file,
                _fileSystem.Path.Combine(target, _fileSystem.Path.GetFileName(file)),
                overwrite: true
            );
        }
    }

    public void ClearDirectory(string directory)
    {
        _fileSystem.Directory.CreateDirectory(directory);

        foreach (var file in _fileSystem.Directory.EnumerateFiles(directory))
        {
            _fileSystem.File.Delete(file);
        }

        foreach (var childDirectory in _fileSystem.Directory.EnumerateDirectories(directory))
        {
            _fileSystem.Directory.Delete(childDirectory, recursive: true);
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

        if (
            targetPath.StartsWith(
                sourcePath + _fileSystem.Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            throw new InvalidOperationException($"Refusing to copy '{sourcePath}' into nested target '{targetPath}'.");
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

        var defaultArtifactsRoot = NormalizeDirectoryPath(_fileSystem.Path.Combine(rootPath, ".artifacts"));
        if (
            !artifactPath.StartsWith(
                defaultArtifactsRoot + _fileSystem.Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            throw new InvalidOperationException(
                $"Refusing to clear custom artifact directory '{artifactPath}'. Choose a path under '{defaultArtifactsRoot}' or use --preserve-existing-artifacts."
            );
        }
    }

    private string NormalizeDirectoryPath(string path) =>
        _fileSystem
            .Path.GetFullPath(path)
            .TrimEnd(_fileSystem.Path.DirectorySeparatorChar, _fileSystem.Path.AltDirectorySeparatorChar);
}
