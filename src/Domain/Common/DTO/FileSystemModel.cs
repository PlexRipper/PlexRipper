namespace PlexRipper.Domain;

public record FileSystemModel
{
    public required FileSystemEntityType Type { get; init; }

    public required string Name { get; init; }

    public required string Path { get; init; }

    public required string Extension { get; init; }

    public required long Size { get; init; }

    public required DateTime? LastModified { get; init; }

    public required bool HasReadPermission { get; init; }

    public required bool HasWritePermission { get; init; }
}
