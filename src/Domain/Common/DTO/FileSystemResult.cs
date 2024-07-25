namespace PlexRipper.Domain;

public record FileSystemResult
{
    public required string Parent { get; init; }

    public required List<FileSystemModel> Directories { get; init; } = new();

    public required List<FileSystemModel> Files { get; init; } = new();

    public required FileSystemModel? Current { get; init; }
}
