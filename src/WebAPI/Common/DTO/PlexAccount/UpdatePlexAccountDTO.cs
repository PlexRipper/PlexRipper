namespace PlexRipper.WebAPI.Common.DTO;

public class UpdatePlexAccountDTO
{
    public int Id { get; set; }

    public string DisplayName { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsMain { get; set; }
}