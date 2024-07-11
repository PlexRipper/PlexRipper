// ReSharper disable once CheckNamespace
// Needs to be in the same namespace as the FluentResults package

namespace FluentResults;

public class PlexError : Error
{
    #region Constructors

    public PlexError(string message)
        : base(message) { }

    #endregion

    #region Properties

    public int Code { get; set; }

    public int Status { get; set; }

    #endregion
}
