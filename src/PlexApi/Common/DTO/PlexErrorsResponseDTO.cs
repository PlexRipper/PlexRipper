using Application.Contracts;

namespace PlexRipper.PlexApi;

public class PlexErrorsResponseDTO
{
    public List<PlexErrorDTO> Errors { get; set; }

    public List<PlexError> ToResultErrors()
    {
        return Errors.Select(x => new PlexError(x.Message)
            {
                Code = x.Code,
                Status = x.Status,
            })
            .ToList();
    }
}

public class PlexErrorDTO
{
    public int Code { get; set; }

    public string Message { get; set; }

    public int Status { get; set; }
}