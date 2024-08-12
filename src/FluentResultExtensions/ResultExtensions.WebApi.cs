using FluentResultExtensions;

// ReSharper disable once CheckNamespace
// Needs to be in the same namespace as the FluentResults package
namespace FluentResults;

public static partial class ResultExtensions
{
    #region Properties

    public static string StatusCodeName => "StatusCode";

    public static string ErrorMessageName => "ErrorMessage";

    #endregion

    #region Implementation

    #region HasStatusCode

    public static bool HasStatusCode(this Result result, int statusCode = 0)
    {
        if (result is null)
            return false;

        foreach (var reason in result.Reasons)
        foreach (var (key, metaData) in reason.Metadata)
            if (key == StatusCodeName && (statusCode == 0 || (int)metaData == statusCode))
                return true;

        return false;
    }

    public static bool HasStatusCode<T>(this Result<T> result, int statusCode) =>
        result?.ToResult()?.HasStatusCode(statusCode) ?? false;

    #endregion

    #region GetStatusCodeError

    public static IReason GetStatusCodeReason(this Result result)
    {
        return result.Reasons.Find(x => x.HasMetadataKey(StatusCodeName));
    }

    public static IReason GetStatusCodeReason<T>(this Result<T> result)
    {
        return result.Reasons.Find(x => x.HasMetadataKey(StatusCodeName));
    }

    #endregion

    #region AddStatusCode

    public static Result AddStatusCode(this Result result, int statusCode, string msg = "")
    {
        return statusCode switch
        {
            200 => result.Add200OkRequestSuccess(msg),
            201 => result.Add201CreatedRequestSuccess(msg),
            400 => result.Add400BadRequestError(msg),
            401 => result.Add401UnauthorizedError(msg),
            404 => result.Add404NotFoundError(msg),
            408 => result.Add408RequestTimeoutError(msg),
            502 => result.Add502BadGatewayError(msg),
            _ => result.AddStatusCodeError(statusCode, msg),
        };
    }

    public static Result<T> AddStatusCode<T>(this Result<T> result, int statusCode, string msg = "")
    {
        return statusCode switch
        {
            200 => result.Add200OkRequestSuccess(msg),
            201 => result.Add201CreatedRequestSuccess(msg),
            400 => result.Add400BadRequestError(msg),
            401 => result.Add401UnauthorizedError(msg),
            404 => result.Add404NotFoundError(msg),
            408 => result.Add408RequestTimeoutError(msg),
            502 => result.Add502BadGatewayError(msg),
            _ => result.AddStatusCodeError(statusCode, msg),
        };
    }

    #endregion

    #region FindStatusCode

    public static int FindStatusCode(this Result result)
    {
        if (result is null)
            return 0;

        foreach (var reason in result.Reasons)
        foreach (var (key, metaData) in reason.Metadata)
            if (key == StatusCodeName)
                return (int)metaData;

        return 0;
    }

    public static bool FindStatusCode<T>(this Result<T> result, int statusCode) =>
        result?.ToResult()?.HasStatusCode(statusCode) ?? false;

    #endregion

    private static Result AddErrorMessageToResult(this Result result, string errorMessage)
    {
        if (result.Errors.Any())
            result.Errors[0].Metadata.Add(ErrorMessageName, errorMessage);

        return result;
    }

    private static Result CreateErrorStatusCodeResult(int statusCode, string message = "") =>
        Result.Fail(GetStatusCodeReason(statusCode, message));

    private static Result CreateSuccessStatusCodeResult(int statusCode, string message = "") =>
        Result.Ok().WithSuccess(GetStatusCodeSuccess(statusCode, message));

    private static Result<T> CreateSuccessStatusCodeResult<T>(T value, int statusCode, string message = "") =>
        Result.Ok(value).WithSuccess(GetStatusCodeSuccess(statusCode, message));

    #region CreateStatusCodeReasons

    private static Error GetStatusCodeReason(int statusCode, string message = "") =>
        new Error(message).WithMetadata(StatusCodeName, statusCode);

    private static Success GetStatusCodeSuccess(int statusCode, string message = "") =>
        new Success(message).WithMetadata(StatusCodeName, statusCode);

    #endregion

    #region AddStatusCodeError

    private static Result AddStatusCodeError(this Result result, int statusCode, string message = "")
    {
        if (string.IsNullOrEmpty(message))
            message = "No error message found";

        if (!result.Errors.Any())
            result.WithError(new Error($"Status code: ({statusCode}) - {message}"));

        result.Errors[0].Metadata.Add(StatusCodeName, statusCode);
        result.Errors[0].Metadata.Add(ErrorMessageName, message);

        return result;
    }

    private static Result<T> AddStatusCodeError<T>(this Result<T> result, int statusCode, string message = "") =>
        result.AddStatusCodeError(GetStatusCodeReason(statusCode, message));

    private static Result<T> AddStatusCodeError<T>(this Result<T> result, Error error) => result.WithError(error);

    #endregion

    #region AddStatusCodeSuccess

    private static Result AddStatusCodeSuccess(this Result result, int statusCode, string message = "") =>
        result.AddStatusCodeSuccess(GetStatusCodeSuccess(statusCode, message));

    private static Result AddStatusCodeSuccess(this Result result, Success Success) => result.WithSuccess(Success);

    private static Result<T> AddStatusCodeSuccess<T>(this Result<T> result, int statusCode, string message = "") =>
        result.AddStatusCodeSuccess(GetStatusCodeSuccess(statusCode, message));

    private static Result<T> AddStatusCodeSuccess<T>(this Result<T> result, Success success) =>
        result.WithSuccess(success);

    #endregion

    #endregion

    #region Result Signatures

    #region General

    public static Result AddErrorMessage(this Result result, string message) => result.AddErrorMessageToResult(message);

    #endregion

    #region 200

    public static bool Has200OkRequestSuccess(this Result result) => result.HasStatusCode(HttpCodes.Status200OK);

    public static Result Add200OkRequestSuccess(this Result result, string message = "Ok successful") =>
        result.AddStatusCodeSuccess(HttpCodes.Status200OK, message);

    public static Result Create200OkResult(string message = "") =>
        CreateSuccessStatusCodeResult(HttpCodes.Status200OK, message);

    #endregion

    #region 201

    public static bool Has201CreatedRequestSuccess(this Result result) =>
        result.HasStatusCode(HttpCodes.Status201Created);

    public static Result Add201CreatedRequestSuccess(this Result result, string message = "Created successful") =>
        result.AddStatusCodeSuccess(HttpCodes.Status201Created, message);

    public static Result Create201CreatedResult(string message = "") =>
        CreateSuccessStatusCodeResult(HttpCodes.Status201Created, message);

    #endregion

    #region 204

    public static bool Has204NoContentRequestSuccess(this Result result) =>
        result.HasStatusCode(HttpCodes.Status204NoContent);

    public static Result Add204NoContentRequestSuccess(this Result result, string message = "No Content") =>
        result.AddStatusCodeSuccess(HttpCodes.Status204NoContent, message);

    public static Result Create204NoContentResult(string message = "") =>
        CreateSuccessStatusCodeResult(HttpCodes.Status204NoContent, message);

    #endregion

    #region 400

    public static bool Has400BadRequestError(this Result result) => result.HasStatusCode(HttpCodes.Status400BadRequest);

    public static Result Add400BadRequestError(this Result result, string message = "Bad request") =>
        result.AddStatusCodeError(HttpCodes.Status400BadRequest, message);

    public static Result Create400BadRequestResult(string message = "") =>
        CreateErrorStatusCodeResult(HttpCodes.Status400BadRequest, message);

    #endregion

    #region 401

    public static bool Has401UnauthorizedError(this Result result) =>
        result.HasStatusCode(HttpCodes.Status401Unauthorized);

    public static Result Add401UnauthorizedError(this Result result, string message = "Unauthorized") =>
        result.AddStatusCodeError(HttpCodes.Status401Unauthorized, message);

    public static Result Create401UnauthorizedResult(string message = "") =>
        CreateErrorStatusCodeResult(HttpCodes.Status401Unauthorized, message);

    #endregion

    #region 403

    public static bool Has403ForbiddenError(this Result result) => result.HasStatusCode(HttpCodes.Status403Forbidden);

    public static Result Add403ForbiddenError(this Result result, string message = "Unauthorized") =>
        result.AddStatusCodeError(HttpCodes.Status403Forbidden, message);

    public static Result Create403ForbiddenResult(string message = "") =>
        CreateErrorStatusCodeResult(HttpCodes.Status403Forbidden, message);

    #endregion

    #region 404

    public static bool Has404NotFoundError(this Result result) => result.HasStatusCode(HttpCodes.Status404NotFound);

    public static Result Add404NotFoundError(this Result result, string message = "Not Found") =>
        result.AddStatusCodeError(HttpCodes.Status404NotFound, message);

    public static Result Create404NotFoundResult(string message = "") =>
        CreateErrorStatusCodeResult(HttpCodes.Status404NotFound, message);

    #endregion

    #region 408

    public static bool Has408RequestTimeout(this Result result) =>
        result.HasStatusCode(HttpCodes.Status408RequestTimeout);

    public static Result Add408RequestTimeoutError(this Result result, string message = "Request Timeout") =>
        result.AddStatusCodeError(HttpCodes.Status408RequestTimeout, message);

    public static Result Create408RequestTimeoutError(string message = "") =>
        CreateErrorStatusCodeResult(HttpCodes.Status408RequestTimeout, message);

    #endregion

    #region 502

    public static bool Has502BadGatewayError(this Result result) => result.HasStatusCode(HttpCodes.Status502BadGateway);

    public static Result Add502BadGatewayError(this Result result, string message = "Not Found") =>
        result.AddStatusCodeError(HttpCodes.Status502BadGateway, message);

    public static Result Create502BadGatewayResult(string message = "") =>
        CreateErrorStatusCodeResult(HttpCodes.Status502BadGateway, message);

    #endregion

    #endregion

    #region Result<T> Signatures

    #region 200

    public static bool Has200OkRequestSuccess<T>(this Result<T> result) => result.HasStatusCode(HttpCodes.Status200OK);

    public static Result<T> Add200OkRequestSuccess<T>(this Result<T> result, string message = "Ok successful") =>
        result.AddStatusCodeSuccess(HttpCodes.Status200OK, message);

    public static Result<T> Create200OkResult<T>(T value, string message = "") =>
        CreateSuccessStatusCodeResult(value, HttpCodes.Status200OK, message);

    #endregion

    #region 201

    public static bool Has201CreatedRequestSuccess<T>(this Result<T> result) =>
        result.HasStatusCode(HttpCodes.Status201Created);

    public static Result<T> Add201CreatedRequestSuccess<T>(
        this Result<T> result,
        string message = "Created successful"
    ) => result.AddStatusCodeSuccess(HttpCodes.Status201Created, message);

    public static Result<T> Create201CreatedResult<T>(T value, string message = "") =>
        CreateSuccessStatusCodeResult(value, HttpCodes.Status201Created, message);

    #endregion

    #region 204

    public static bool Has204NoContentRequestSuccess<T>(this Result<T> result) =>
        result.HasStatusCode(HttpCodes.Status204NoContent);

    public static Result<T> Add204NoContentRequestSuccess<T>(this Result<T> result, string message = "No Content") =>
        result.AddStatusCodeSuccess(HttpCodes.Status204NoContent, message);

    public static Result<T> Create204NoContentResult<T>(T value, string message = "") =>
        CreateSuccessStatusCodeResult(value, HttpCodes.Status204NoContent, message);

    #endregion

    #region 400

    public static bool Has400BadRequestError<T>(this Result<T> result) =>
        result.HasStatusCode(HttpCodes.Status400BadRequest);

    public static Result<T> Add400BadRequestError<T>(this Result<T> result, string message = "Bad request") =>
        result.AddStatusCodeError(HttpCodes.Status400BadRequest, message);

    #endregion

    #region 401

    public static bool Has401UnauthorizedError<T>(this Result<T> result) =>
        result.HasStatusCode(HttpCodes.Status401Unauthorized);

    public static Result<T> Add401UnauthorizedError<T>(this Result<T> result, string message = "Unauthorized") =>
        result.AddStatusCodeError(HttpCodes.Status401Unauthorized, message);

    #endregion

    #region 403

    public static bool Has403ForbiddenError<T>(this Result<T> result) =>
        result.HasStatusCode(HttpCodes.Status403Forbidden);

    public static Result<T> Add403ForbiddenError<T>(this Result<T> result, string message = "Forbidden") =>
        result.AddStatusCodeError(HttpCodes.Status403Forbidden, message);

    #endregion

    #region 404

    public static bool Has404NotFoundError<T>(this Result<T> result) =>
        result.HasStatusCode(HttpCodes.Status404NotFound);

    public static Result<T> Add404NotFoundError<T>(this Result<T> result, string message = "Not Found") =>
        result.AddStatusCodeError(HttpCodes.Status404NotFound, message);

    #endregion

    #region 408

    public static bool Has408RequestTimeout<T>(this Result<T> result) =>
        result.HasStatusCode(HttpCodes.Status408RequestTimeout);

    public static Result<T> Add408RequestTimeoutError<T>(this Result<T> result, string message = "Request Timeout") =>
        result.AddStatusCodeError(HttpCodes.Status408RequestTimeout, message);

    #endregion

    #region 502

    public static bool Has502BadGatewayError<T>(this Result<T> result) =>
        result.HasStatusCode(HttpCodes.Status502BadGateway);

    public static Result<T> Add502BadGatewayError<T>(this Result<T> result, string message = "Not Found") =>
        result.AddStatusCodeError(HttpCodes.Status502BadGateway, message);

    #endregion

    #endregion
}
