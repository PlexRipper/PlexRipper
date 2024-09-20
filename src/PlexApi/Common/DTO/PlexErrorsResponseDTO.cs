namespace PlexRipper.PlexApi;

public class PlexErrorsResponseDTO
{
    public List<PlexErrorDTO> Errors { get; set; } = new();

    public List<PlexError> ToResultErrors()
    {
        return Errors.Select(x => new PlexError(x.Message) { Code = x.Code, Status = x.Status, }).ToList();
    }
}
