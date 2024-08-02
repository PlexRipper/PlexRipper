namespace Application.Contracts;

public class FileSystemDTO
{
    public required string Parent { get; init; } = string.Empty;

    public required List<FileSystemModelDTO> Directories { get; init; }

    public required List<FileSystemModelDTO> Files { get; init; }

    public required FileSystemModelDTO? Current { get; init; }
}
