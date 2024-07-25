using PlexRipper.Domain;

namespace FileSystem.Contracts;

public static class FileInfoMappers
{
    public static FileSystemModel ToModel(this FileInfo source) =>
        new()
        {
            Name = source.Name,
            Path = source.FullName.GetActualCasing(),
            LastModified = source.LastWriteTimeUtc,
            Extension = source.Extension,
            Size = source.Length,
            Type = FileSystemEntityType.File,
            HasReadPermission = source.IsReadOnly, // TODO: check if this is correct
            HasWritePermission = !source.IsReadOnly,
        };
}
