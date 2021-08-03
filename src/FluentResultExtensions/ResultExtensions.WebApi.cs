using FluentResultExtensions.lib;

// ReSharper disable once CheckNamespace
namespace FluentResults
{
    public static partial class ResultExtensions
    {
        public static string StatusCodeName => "StatusCode";

        #region Implementation

        #region FindStatusCode

        private static bool FindStatusCode(this Result result, int statusCode)
        {
            if (result is null || statusCode <= 0)
            {
                return false;
            }

            foreach (Reason reason in result.Reasons)
            {
                foreach (var (key, metaData) in reason.Metadata)
                {
                    if (key == StatusCodeName && (int)metaData == statusCode)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool FindStatusCode<T>(this Result<T> result, int statusCode)
        {
            return result?.ToResult()?.FindStatusCode(statusCode) ?? false;
        }

        #endregion

        private static Result CreateErrorStatusCodeResult(int statusCode, string message = "")
        {
            return Result.Fail(GetStatusCodeError(statusCode, message));
        }

        private static Result CreateSuccessStatusCodeResult(int statusCode, string message = "")
        {
            return Result.Ok().WithSuccess(GetStatusCodeSuccess(statusCode, message));
        }

        private static Result<T> CreateSuccessStatusCodeResult<T>(T value, int statusCode, string message = "")
        {
            return Result.Ok(value).WithSuccess(GetStatusCodeSuccess(statusCode, message));
        }

        #region CreateStatusCodeReasons

        private static Error GetStatusCodeError(int statusCode, string message = "")
        {
            return new Error(message).WithMetadata(StatusCodeName, statusCode);
        }

        private static Success GetStatusCodeSuccess(int statusCode, string message = "")
        {
            return new Success(message).WithMetadata(StatusCodeName, statusCode);
        }

        #endregion

        #region AddStatusCodeError

        private static Result AddStatusCodeError(this Result result, int statusCode, string message = "")
        {
            return result.AddStatusCodeError(GetStatusCodeError(statusCode, message));
        }

        private static Result AddStatusCodeError(this Result result, Error error)
        {
            return result.WithError(error);
        }

        private static Result<T> AddStatusCodeError<T>(this Result<T> result, int statusCode, string message = "")
        {
            return result.AddStatusCodeError(GetStatusCodeError(statusCode, message));
        }

        private static Result<T> AddStatusCodeError<T>(this Result<T> result, Error error)
        {
            return result.WithError(error);
        }

        #endregion

        #region AddStatusCodeSuccess

        private static Result AddStatusCodeSuccess(this Result result, int statusCode, string message = "")
        {
            return result.AddStatusCodeSuccess(GetStatusCodeSuccess(statusCode, message));
        }

        private static Result AddStatusCodeSuccess(this Result result, Success Success)
        {
            return result.WithSuccess(Success);
        }

        private static Result<T> AddStatusCodeSuccess<T>(this Result<T> result, int statusCode, string message = "")
        {
            return result.AddStatusCodeSuccess(GetStatusCodeSuccess(statusCode, message));
        }

        private static Result<T> AddStatusCodeSuccess<T>(this Result<T> result, Success success)
        {
            return result.WithSuccess(success);
        }

        #endregion

        #region 201

        private static bool _has201CreatedRequestSuccess(Result result)
        {
            return FindStatusCode(result, HttpCodes.Status201Created);
        }

        #endregion

        #endregion

        #region Result Signatures

        #region 201

        public static bool Has201CreatedRequestSuccess(this Result result)
        {
            return result.FindStatusCode(HttpCodes.Status201Created);
        }

        public static Result Add201CreatedRequestSuccess(this Result result, string message = "Created successful")
        {
            return result.AddStatusCodeSuccess(HttpCodes.Status201Created, message);
        }

        public static Result Create201CreatedResult(string message = "")
        {
            return CreateSuccessStatusCodeResult(HttpCodes.Status201Created, message);
        }

        #endregion

        #region 400

        public static bool Has400BadRequestError(this Result result)
        {
            return result.FindStatusCode(HttpCodes.Status400BadRequest);
        }

        public static Result Add400BadRequestError(this Result result, string message = "Bad request")
        {
            return result.AddStatusCodeError(HttpCodes.Status400BadRequest, message);
        }

        public static Result Create400BadRequestResult(string message = "")
        {
            return CreateErrorStatusCodeResult(HttpCodes.Status400BadRequest, message);
        }

        #endregion

        #region 404

        public static bool Has404NotFoundError(this Result result)
        {
            return result.FindStatusCode(HttpCodes.Status404NotFound);
        }

        public static Result Add404NotFoundError(this Result result, string message = "Not Found")
        {
            return result.AddStatusCodeError(HttpCodes.Status404NotFound, message);
        }

        public static Result Create404NotFoundResult(string message = "")
        {
            return CreateErrorStatusCodeResult(HttpCodes.Status404NotFound, message);
        }

        #endregion

        #endregion

        #region Result<T> Signatures

        #region 201

        public static bool Has201CreatedRequestSuccess<T>(this Result<T> result)
        {
            return _has201CreatedRequestSuccess(result);
        }

        public static Result<T> Add201CreatedRequestSuccess<T>(this Result<T> result, string message = "Created successful")
        {
            return result.AddStatusCodeSuccess(HttpCodes.Status201Created, message);
        }

        public static Result<T> Create201CreatedResult<T>(T value, string message = "")
        {
            return CreateSuccessStatusCodeResult(value, HttpCodes.Status201Created, message);
        }

        #endregion

        #region 400

        public static bool Has400BadRequestError<T>(this Result<T> result)
        {
            return result.FindStatusCode(HttpCodes.Status400BadRequest);
        }

        public static Result<T> Add400BadRequestError<T>(this Result<T> result, string message = "Bad request")
        {
            return result.AddStatusCodeError(HttpCodes.Status400BadRequest, message);
        }

        #endregion

        #region 404

        public static bool Has404NotFoundError<T>(this Result<T> result)
        {
            return result.FindStatusCode(HttpCodes.Status404NotFound);
        }

        public static Result<T> Add404NotFoundError<T>(this Result<T> result, string message = "Not Found")
        {
            return result.AddStatusCodeError(HttpCodes.Status404NotFound, message);
        }

        #endregion

        #endregion
    }
}