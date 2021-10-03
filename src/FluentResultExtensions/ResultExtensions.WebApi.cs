using FluentResultExtensions.lib;

// ReSharper disable once CheckNamespace
namespace FluentResults
{
    public static partial class ResultExtensions
    {
        public static string StatusCodeName => "StatusCode";

        #region Implementation

        #region HasStatusCode

        public static bool HasStatusCode(this Result result, int statusCode = 0)
        {
            if (result is null)
            {
                return false;
            }

            foreach (Reason reason in result.Reasons)
            {
                foreach (var (key, metaData) in reason.Metadata)
                {
                    if (key == StatusCodeName && (statusCode == 0 || (int)metaData == statusCode))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool HasStatusCode<T>(this Result<T> result, int statusCode)
        {
            return result?.ToResult()?.HasStatusCode(statusCode) ?? false;
        }

        #endregion

        #region AddStatusCode

        public static Result AddStatusCode(this Result result, int statusCode)
        {
            return statusCode switch
            {
                200 => result.Add200OkRequestSuccess(),
                201 => result.Add201CreatedRequestSuccess(),
                400 => result.Add400BadRequestError(),
                401 => result.Add401UnauthorizedError(),
                404 => result.Add404NotFoundError(),
                408 => result.Add408RequestTimeoutError(),
                502 => result.Add502BadGatewayError(),
                _ => result.AddStatusCodeError(statusCode),
            };
        }

        public static Result<T> AddStatusCode<T>(this Result<T> result, int statusCode)
        {
            return statusCode switch
            {
                200 => result.Add200OkRequestSuccess(),
                201 => result.Add201CreatedRequestSuccess(),
                400 => result.Add400BadRequestError(),
                401 => result.Add401UnauthorizedError(),
                404 => result.Add404NotFoundError(),
                408 => result.Add408RequestTimeoutError(),
                502 => result.Add502BadGatewayError(),
                _ => result.AddStatusCodeError(statusCode),
            };
        }

        #endregion

        #region FindStatusCode

        public static int FindStatusCode(this Result result)
        {
            if (result is null)
            {
                return 0;
            }

            foreach (Reason reason in result.Reasons)
            {
                foreach (var (key, metaData) in reason.Metadata)
                {
                    if (key == StatusCodeName)
                    {
                        return (int)metaData;
                    }
                }
            }

            return 0;
        }

        public static bool FindStatusCode<T>(this Result<T> result, int statusCode)
        {
            return result?.ToResult()?.HasStatusCode(statusCode) ?? false;
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

        #endregion

        #region Result Signatures

        #region 200

        public static bool Has200OkRequestSuccess(this Result result)
        {
            return result.HasStatusCode(HttpCodes.Status200OK);
        }

        public static Result Add200OkRequestSuccess(this Result result, string message = "Ok successful")
        {
            return result.AddStatusCodeSuccess(HttpCodes.Status200OK, message);
        }

        public static Result Create200OkResult(string message = "")
        {
            return CreateSuccessStatusCodeResult(HttpCodes.Status200OK, message);
        }

        #endregion

        #region 201

        public static bool Has201CreatedRequestSuccess(this Result result)
        {
            return result.HasStatusCode(HttpCodes.Status201Created);
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
            return result.HasStatusCode(HttpCodes.Status400BadRequest);
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

        #region 401

        public static bool Has401UnauthorizedError(this Result result)
        {
            return result.HasStatusCode(HttpCodes.Status401Unauthorized);
        }

        public static Result Add401UnauthorizedError(this Result result, string message = "Unauthorized")
        {
            return result.AddStatusCodeError(HttpCodes.Status401Unauthorized, message);
        }

        public static Result Create401UnauthorizedResult(string message = "")
        {
            return CreateErrorStatusCodeResult(HttpCodes.Status401Unauthorized, message);
        }

        #endregion

        #region 404

        public static bool Has404NotFoundError(this Result result)
        {
            return result.HasStatusCode(HttpCodes.Status404NotFound);
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

        #region 408

        public static bool Has408RequestTimeout(this Result result)
        {
            return result.HasStatusCode(HttpCodes.Status408RequestTimeout);
        }

        public static Result Add408RequestTimeoutError(this Result result, string message = "Request Timeout")
        {
            return result.AddStatusCodeError(HttpCodes.Status408RequestTimeout, message);
        }

        public static Result Create408RequestTimeoutError(string message = "")
        {
            return CreateErrorStatusCodeResult(HttpCodes.Status408RequestTimeout, message);
        }

        #endregion

        #region 502

        public static bool Has502BadGatewayNotFoundError(this Result result)
        {
            return result.HasStatusCode(HttpCodes.Status502BadGateway);
        }

        public static Result Add502BadGatewayError(this Result result, string message = "Not Found")
        {
            return result.AddStatusCodeError(HttpCodes.Status502BadGateway, message);
        }

        public static Result Create502BadGatewayResult(string message = "")
        {
            return CreateErrorStatusCodeResult(HttpCodes.Status502BadGateway, message);
        }

        #endregion

        #endregion

        #region Result<T> Signatures

        #region 200

        public static bool Has200OkRequestSuccess<T>(this Result<T> result)
        {
            return result.HasStatusCode(HttpCodes.Status200OK);
        }

        public static Result<T> Add200OkRequestSuccess<T>(this Result<T> result, string message = "Ok successful")
        {
            return result.AddStatusCodeSuccess(HttpCodes.Status200OK, message);
        }

        public static Result<T> Create200OkResult<T>(T value, string message = "")
        {
            return CreateSuccessStatusCodeResult(value, HttpCodes.Status200OK, message);
        }

        #endregion

        #region 201

        public static bool Has201CreatedRequestSuccess<T>(this Result<T> result)
        {
            return result.HasStatusCode(HttpCodes.Status201Created);
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
            return result.HasStatusCode(HttpCodes.Status400BadRequest);
        }

        public static Result<T> Add400BadRequestError<T>(this Result<T> result, string message = "Bad request")
        {
            return result.AddStatusCodeError(HttpCodes.Status400BadRequest, message);
        }

        #endregion

        #region 401

        public static bool Has401UnauthorizedError<T>(this Result<T> result)
        {
            return result.HasStatusCode(HttpCodes.Status401Unauthorized);
        }

        public static Result<T> Add401UnauthorizedError<T>(this Result<T> result, string message = "Unauthorized")
        {
            return result.AddStatusCodeError(HttpCodes.Status401Unauthorized, message);
        }

        #endregion

        #region 404

        public static bool Has404NotFoundError<T>(this Result<T> result)
        {
            return result.HasStatusCode(HttpCodes.Status404NotFound);
        }

        public static Result<T> Add404NotFoundError<T>(this Result<T> result, string message = "Not Found")
        {
            return result.AddStatusCodeError(HttpCodes.Status404NotFound, message);
        }

        #endregion

        #region 408

        public static bool Has408RequestTimeout<T>(this Result<T> result)
        {
            return result.HasStatusCode(HttpCodes.Status408RequestTimeout);
        }

        public static Result<T> Add408RequestTimeoutError<T>(this Result<T> result, string message = "Request Timeout")
        {
            return result.AddStatusCodeError(HttpCodes.Status408RequestTimeout, message);
        }

        #endregion

        #region 502

        public static bool Has502BadGatewayNotFoundError<T>(this Result<T> result)
        {
            return result.HasStatusCode(HttpCodes.Status502BadGateway);
        }

        public static Result<T> Add502BadGatewayError<T>(this Result<T> result, string message = "Not Found")
        {
            return result.AddStatusCodeError(HttpCodes.Status502BadGateway, message);
        }

        #endregion

        #endregion
    }
}