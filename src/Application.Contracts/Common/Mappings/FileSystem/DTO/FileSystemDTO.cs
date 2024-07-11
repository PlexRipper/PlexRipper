namespace Application.Contracts;

public class FileSystemDTO
{
    public required string Parent { get; set; } = string.Empty;

    public required List<FileSystemModelDTO> Directories { get; set; }

    public required List<FileSystemModelDTO> Files { get; set; }
}
