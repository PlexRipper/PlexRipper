// ReSharper disable once CheckNamespace
// Needs to be in the same namespace as the FluentResults package

namespace FluentResults;

public static partial class ResultExtensions
{
    public static int StatusCode(this IReason error)
    {
        return error.HasMetadataKey(StatusCodeName) ? Convert.ToInt32(error.Metadata[StatusCodeName]) : 0;
    }

    public static string ErrorMessage(this IReason error)
    {
        return error.HasMetadataKey(ErrorMessageName) ? error.Metadata[ErrorMessageName]?.ToString() : "Message not found";
    }
}