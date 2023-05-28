namespace PlexRipper.WebAPI.Common.DTO.FolderPath;

public class FileSystemDTO
{
    public string Parent { get; set; }

    public List<FileSystemModelDTO> Directories { get; set; }

    public List<FileSystemModelDTO> Files { get; set; }
}