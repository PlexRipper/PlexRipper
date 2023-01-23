namespace PlexRipper.WebAPI.Common.DTO;

public class PlexServerStatusDTO
{
    public int Id { get; set; }

    public int StatusCode { get; set; }

    public bool IsSuccessful { get; set; }

    public string StatusMessage { get; set; }

    public DateTime LastChecked { get; set; }

    public int PlexServerId { get; set; }
}