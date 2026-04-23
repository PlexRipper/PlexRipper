namespace Reaparr.Build;

internal static class FileSystemTasks
{
    public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
    {
        foreach (var directory in source.EnumerateDirectories())
        {
            CopyDirectory(directory, target.CreateSubdirectory(directory.Name));
        }

        foreach (var file in source.EnumerateFiles())
        {
            file.CopyTo(Path.Combine(target.FullName, file.Name), overwrite: true);
        }
    }

    public static void ClearDirectory(DirectoryInfo directory)
    {
        directory.Create();

        foreach (var file in directory.EnumerateFiles())
        {
            file.Delete();
        }

        foreach (var childDirectory in directory.EnumerateDirectories())
        {
            childDirectory.Delete(recursive: true);
        }
    }
}
