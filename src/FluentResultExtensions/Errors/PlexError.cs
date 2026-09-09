namespace Reaparr.FluentResultExtensions;

public class PlexError : Error
{
    public PlexError(string message)
        : base(message) { }

    public int Code { get; set; }

    public int Status { get; set; }
}
