using PlexRipper.Domain;

namespace FileSystem.Contracts;

public static class DirectoryInfoMappers
{
    public static FileSystemModel ToModel(this DirectoryInfo source) =>
        new()
        {
            Name = source.Name,
            Path = GetDirectoryPath(source.FullName.GetActualCasing()),
            LastModified = source.LastWriteTimeUtc,
            Type = FileSystemEntityType.Folder,
            Extension = source.Extension,
            Size = 0, // TODO maybe calculating this is a bit expensive, see if needed
            HasReadPermission = source.CanRead(),
            HasWritePermission = source.CanWrite(),
        };

    public static string GetDirectoryPath(string path)
    {
        if (path.Last() != Path.DirectorySeparatorChar)
            path += Path.DirectorySeparatorChar;

        return path;
    }
}
