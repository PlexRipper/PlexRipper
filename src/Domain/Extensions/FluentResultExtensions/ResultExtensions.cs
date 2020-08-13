using System.Runtime.CompilerServices;
using FluentResults;

namespace PlexRipper.Domain
{
    public static class ResultExtensions
    {
        private static string _statusCode = "StatusCode";

        #region Implementations

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

        private static Result SetStatusCode(Result result, int statusCode, string message = "")
        {
            return result.WithError(new Error(message).WithMetadata(_statusCode, HttpCodes.Status400BadRequest));
        }

        #region General

        private static Result _IsNull(this Result result, string parameterName = "")
        {
            return result.WithError(new Error($"The {parameterName} parameter is null."));
        }

        private static Result _IsEmpty(this Result result, string parameterName)
        {
            return result.WithError(new Error($"The {parameterName} parameter was empty"));
        }

        private static Result _IsInvalidId(this Result result, string parameterName, int value = 0)
        {
            return result.WithError(new Error($"The {parameterName} parameter contained an invalid id of {value}")).b;
        }

        private static Result _LogWarning(this Result result, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            foreach (var error in result.Errors)
            {
                Log.Warning(error.Message, memberName, sourceFilePath);
            }
            return result;
        }

        private static Result _LogError(this Result result, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            foreach (var error in result.Errors)
            {
                Log.Error(error.Message, memberName, sourceFilePath);
            }
            return result;
        }

        #endregion


        #region 400

        private static bool _has400Error(Result result)
        {
            return FindStatusCode(result, HttpCodes.Status400BadRequest);
        }

        private static Result _set400Error(Result result, string message = "")
        {
            return SetStatusCode(result, HttpCodes.Status400BadRequest, message);
        }

        #endregion

        #region 404

        private static bool _has404Error(Result result)
        {
            return FindStatusCode(result, HttpCodes.Status404NotFound);
        }

        private static void _set404Error(Result result, string message = "")
        {
            SetStatusCode(result, HttpCodes.Status404NotFound, message);
        }

        #endregion

        #endregion

        #region Result Signatures

        #region General

        public static Result IsNull(this Result result, string parameterName = "")
        {
            return _IsNull(result, parameterName);
        }


        public static Result IsInvalidId(this Result result, string parameterName, int value = 0)
        {
            return _IsInvalidId(result, parameterName, value);
        }

        public static Result IsEmpty(this Result result, string parameterName)
        {
            return _IsEmpty(result, parameterName);
        }

        public static Result LogWarning(this Result result, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return _LogWarning(result, memberName, sourceFilePath);
        }

        public static Result LogError(this Result result, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return _LogError(result, memberName, sourceFilePath);
        }

        #endregion

        #region With New Result

        public static Result IsNull(string parameterName = "")
        {
            return _IsNull(new Result(), parameterName);
        }

        public static Result IsEmpty(string parameterName)
        {
            return _IsEmpty(new Result(), parameterName);
        }

        public static Result IsInvalidId(string parameterName, int value = 0)
        {
            return _IsInvalidId(new Result(), parameterName, value);
        }

        #endregion

        #region 400

        public static bool Has400BadRequestError(this Result result)
        {
            return _has400Error(result);
        }

        public static Result Set400BadRequestError(this Result result, string message = "")
        {
            return _set400Error(result, message);
        }

        public static Result Get400BadRequestResult(string message = "")
        {
            return _set400Error(new Result(), message) ;
        }

        #endregion

        #region 404

        public static bool Has404NotFoundError(this Result result)
        {
            return _has404Error(result);
        }

        public static Result Set404NotFoundError(this Result result, string message = "")
        {
            _set404Error(result, message);
            return result;
        }

        public static Result Get404NotFoundResult()
        {
            return new Result().Set404NotFoundError();
        }

        #endregion

        #endregion

        #region Result<T> Signatures

        #region General

        public static Result<T> IsNull<T>(this Result<T> result, string parameterName = "")
        {
            return result.WithError(new Error($"The {parameterName} parameter is null."));
        }

        public static Result<T> IsNull<T>(string parameterName = "")
        {
            return new Result().IsNull(parameterName);
        }

        public static Result<T> IsInvalidId<T>(this Result<T> result, string parameterName, int value = 0)
        {
            return result.WithError(new Error($"The {parameterName} parameter contained an invalid id of {value}"));
        }

        public static Result<T> IsInvalidId<T>(string parameterName, int value = 0)
        {
            return new Result().IsInvalidId(parameterName, value);
        }

        public static Result<T> IsEmpty<T>(this Result<T> result, string parameterName)
        {
            return IsEmpty(result, parameterName);
        }

        public static Result<T> IsEmpty<T>(string parameterName)
        {
            return IsEmpty(parameterName);
        }

        public static Result<T> LogWarning<T>(this Result<T> result, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return _LogWarning(result, memberName, sourceFilePath);
        }

        public static Result<T> LogError<T>(this Result<T> result, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return _LogError(result, memberName, sourceFilePath);
        }

        #endregion

        #region 400

        public static bool Has400BadRequestError<T>(this Result<T> result)
        {
            return _has400Error(result);
        }

        public static void Set400BadRequestError<T>(this Result<T> result)
        {
            _set400Error(result);
        }

        #endregion


        #region 404

        public static bool Has404NotFoundError<T>(this Result<T> result)
        {
            return _has404Error(result);
        }

        public static void Set404NotFoundError<T>(this Result<T> result)
        {
            _set404Error(result);
        }

        #endregion

        #endregion
    }
}