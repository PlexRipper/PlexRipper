// ReSharper disable once CheckNamespace
// Needs to be in the same namespace as the FluentResults package

namespace FluentResults;

public class PlexError : Error
{
    public PlexError(string message)
        : base(message) { }

    public int Code { get; set; }

    public int Status { get; set; }
}
