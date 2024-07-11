namespace PlexRipper.Domain;

public record FileSystemModel
{
    public required FileSystemEntityType Type { get; set; }

    public required string Name { get; set; }

    public required string Path { get; set; }

    public required string Extension { get; set; }

    public required long Size { get; set; }

    public required DateTime? LastModified { get; set; }
}
