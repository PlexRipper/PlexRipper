using FluentResultExtensions;

// ReSharper disable once CheckNamespace
// Needs to be in the same namespace as the FluentResults package
namespace FluentResults;

public static partial class ResultExtensions
{
    #region Methods

    #region Public

    public static bool HasPlexError<T>(this Result<T> result) => result.ToResult()?.HasPlexError() ?? false;

    public static bool HasPlexError(this Result result) => result.HasError<PlexError>();

    public static List<PlexError> GetPlexErrors(this Result result) => result.Errors.OfType<PlexError>().ToList();

    #endregion

    #endregion

    #region Errors

    public static bool HasPlexErrorInvalidVerificationCode(this Result result)
    {
        return result.HasPlexError()
            && result.GetPlexErrors().Any(x => x.Code == PlexErrorCodes.InvalidVerificationCode);
    }

    public static bool HasPlexErrorInvalidVerificationCode<T>(this Result<T> result) =>
        result.ToResult().HasPlexErrorInvalidVerificationCode();

    #region EnterVerificationCode

    public static bool HasPlexErrorEnterVerificationCode(this Result result)
    {
        return result.HasPlexError() && result.GetPlexErrors().Any(x => x.Code == PlexErrorCodes.EnterVerificationCode);
    }

    public static bool HasPlexErrorEnterVerificationCode<T>(this Result<T> result) =>
        result.ToResult().HasPlexErrorEnterVerificationCode();

    #endregion

    #endregion
}
