using FluentResults;

namespace Application.Contracts;

public class PlexErrorsResponse
{
    public List<PlexError> Errors { get; set; }
}

public class PlexError : Error
{
    public PlexError(string message) : base(message) { }

    public int Code { get; set; }

    public int Status { get; set; }
}