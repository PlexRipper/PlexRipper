namespace PlexRipper.WebAPI.Common.DTO.FolderPath;

public class FolderPathDTO
{
    public int Id { get; set; }

    public FolderType FolderType { get; set; }

    public PlexMediaType MediaType { get; set; }

    public string DisplayName { get; set; }

    public string Directory { get; set; }

    public bool IsValid { get; set; }
}