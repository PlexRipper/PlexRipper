// ReSharper disable once CheckNamespace
// Needs to be in the same namespace as the FluentResults package

namespace FluentResults;

public static partial class ResultExtensions
{
    public static int GetStatusCode(this IReason error) =>
        error.HasMetadataKey(StatusCodeName) ? Convert.ToInt32(error.Metadata[StatusCodeName]) : 0;
}
