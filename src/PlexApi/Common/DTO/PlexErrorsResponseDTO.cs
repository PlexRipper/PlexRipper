using System.Text.Json.Serialization;
using Application.Contracts;
using PlexRipper.Application;

namespace PlexRipper.PlexApi;

public class PlexErrorsResponseDTO
{
    [JsonPropertyName("errors")]
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
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }
}