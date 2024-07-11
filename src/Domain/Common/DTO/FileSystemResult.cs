namespace PlexRipper.Domain;

public class FileSystemResult
{
    public string Parent { get; set; }

    public List<FileSystemModel> Directories { get; set; } = new();

    public List<FileSystemModel> Files { get; set; } = new();
}
