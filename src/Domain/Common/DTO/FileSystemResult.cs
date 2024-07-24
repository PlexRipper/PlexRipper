namespace PlexRipper.Domain;

public record FileSystemResult
{
    public required string Parent { get; set; }

    public required List<FileSystemModel> Directories { get; set; } = new();

    public required List<FileSystemModel> Files { get; set; } = new();

    public required FileSystemModel? Current { get; set; }
}
