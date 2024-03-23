namespace Application.Contracts;

public class FileSystemDTO
{
    public string Parent { get; set; } = string.Empty;

    public List<FileSystemModelDTO> Directories { get; set; }

    public List<FileSystemModelDTO> Files { get; set; }
}