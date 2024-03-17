using PlexRipper.Domain;

namespace Application.Contracts.FolderPath;

public class FileSystemModelDTO
{
    public FileSystemEntityType Type { get; set; }

    public string Name { get; set; }

    public string Path { get; set; }

    public string Extension { get; set; }

    public long Size { get; set; }

    public DateTime? LastModified { get; set; }
}