using FluentResults;

namespace PlexRipper.Domain
{
    public static partial class ResultExtensions
    {
        private static string _statusCode = "StatusCode";

        #region Implementation

        private static bool FindStatusCode(Result result, int statusCode)
        {
            foreach (Error error in result.Errors)
            {
                foreach (var (key, metaData) in error.Metadata)
                {
                    if (key == _statusCode && (int) metaData == statusCode)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static Result _AddStatusCode(Result result, int statusCode, string message = "")
        {
            return result.WithError(GetStatusCodeError(statusCode, message));
        }

        private static Result _AddStatusCode(Result result, Error error)
        {
            return result.WithError(error);
        }

        private static Result _CreateStatusCode(int statusCode, string message = "")
        {
            return Result.Fail(GetStatusCodeError(statusCode, message));
        }

        #region 400

        private static bool _has400BadRequestError(Result result)
        {
            return FindStatusCode(result, HttpCodes.Status400BadRequest);
        }

        private static Result _add400BadRequestError(Result result, string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = "Could not find object";
            }

            return _AddStatusCode(result, Get400BadRequestError(message));
        }

        private static Result _create400BadRequestResult(string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = "Could not find object";
            }
            return _CreateStatusCode(HttpCodes.Status400BadRequest, message);
        }

        #endregion

        #region 404

        private static bool _has404Error(Result result)
        {
            return FindStatusCode(result, HttpCodes.Status404NotFound);
        }

        private static void _set404Error(Result result, string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = "Could not find object";
            }

            _AddStatusCode(result, Get404NotFoundError(message));
        }

        private static Result _create404NotFoundResult(string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = "Could not find object";
            }
            return _CreateStatusCode(HttpCodes.Status400BadRequest, message);
        }

        #endregion

        #endregion

        #region Errors

        public static Error GetStatusCodeError(int statusCode, string message = "")
        {
            return new Error(message).WithMetadata(_statusCode, statusCode);
        }

        public static Error Get400BadRequestError(string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = "Bad Request Error";
            }

            return GetStatusCodeError(HttpCodes.Status400BadRequest, message);
        }

        public static Error Get404NotFoundError(string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = "Bad Request Error";
            }
            return GetStatusCodeError(HttpCodes.Status404NotFound, message);
        }

        #endregion

        #region General

        #region 400

        public static Result Create400BadRequestResult(string message = "")
        {
            return _CreateStatusCode(HttpCodes.Status400BadRequest, message);
        }

        #endregion

        #region 404

        public static Result Create404NotFoundResult(string message = "")
        {
            return _CreateStatusCode(HttpCodes.Status404NotFound, message);
        }

        #endregion

        #endregion

        #region Result Signatures

        #region 400

        public static bool Has400BadRequestError(this Result result)
        {
            return _has400BadRequestError(result);
        }

        public static Result Add400BadRequestError(this Result result, string message = "")
        {
            return _add400BadRequestError(result, message);
        }


        #endregion

        #region 404

        public static bool Has404NotFoundError(this Result result)
        {
            return _has404Error(result);
        }

        public static Result Add404NotFoundError(this Result result, string message = "")
        {
            _set404Error(result, message);
            return result;
        }

        #endregion

        #endregion

        #region Result<T> Signatures

        #region 400

        public static bool Has400BadRequestError<T>(this Result<T> result)
        {
            return _has400BadRequestError(result);
        }

        public static void Add400BadRequestError<T>(this Result<T> result)
        {
            _add400BadRequestError(result);
        }

        #endregion


        #region 404

        public static bool Has404NotFoundError<T>(this Result<T> result)
        {
            return _has404Error(result);
        }

        public static void Add404NotFoundError<T>(this Result<T> result)
        {
            _set404Error(result);
        }

        #endregion

        #endregion
    }
}