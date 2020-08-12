using System.Runtime.CompilerServices;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Domain.FluentResultExtensions
{
    public static class ResultExtensions
    {
        private static string _statusCode = "StatusCode";

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

        private static void SetStatusCode(Result result, int statusCode, string message = "")
        {
            result.WithError(new Error(message).WithMetadata(_statusCode, HttpCodes.Status400BadRequest));
        }

        private static bool _has400Error(Result result)
        {
            return FindStatusCode(result, HttpCodes.Status400BadRequest);
        }

        private static bool _has404Error(Result result)
        {
            return FindStatusCode(result, HttpCodes.Status404NotFound);
        }

        private static void _set400Error(Result result, string message = "")
        {
            SetStatusCode(result, HttpCodes.Status400BadRequest, message);
        }

        private static void _set404Error(Result result, string message = "")
        {
            SetStatusCode(result, HttpCodes.Status404NotFound, message);
        }

        #region Result Signatures

        #region General

        public static Result IsNull(this Result result, string parameterName = "")
        {
            return result.WithError(new Error($"The {parameterName} parameter is null."));
        }

        public static Result IsNull(string parameterName = "")
        {
            return new Result().IsNull(parameterName);
        }

        public static Result IsInvalidId(this Result result, string parameterName, int value = 0)
        {
            return result.WithError(new Error($"The {parameterName} parameter contained an invalid id of {value}"));
        }

        public static Result IsInvalidId(string parameterName, int value = 0)
        {
            return new Result().IsInvalidId(parameterName, value);
        }

        public static Result IsEmpty(this Result result, string parameterName)
        {
            return result.WithError(new Error($"The {parameterName} parameter was empty"));
        }

        public static Result IsEmpty(string parameterName)
        {
            return new Result().IsEmpty(parameterName);
        }

        public static Result LogWarning(this Result result, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            foreach (var error in result.Errors)
            {
                Log.Warning(error.Message, memberName, sourceFilePath);
            }
            return result;
        }

        public static Result LogError(this Result result, [CallerMemberName] string memberName = "",
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

        public static bool Has400Error(this Result result)
        {
            return _has400Error(result);
        }

        public static void Set400Error(this Result result)
        {
            _set400Error(result);
        }

        #endregion

        #region 404

        public static bool Has404NotFoundError(this Result result)
        {
            return _has404Error(result);
        }

        public static Result Set404NotFoundError(this Result result)
        {
            _set404Error(result);
            return result;
        }

        public static Result Get404NotFoundError()
        {
            return new Result().Set404NotFoundError();
        }

        #endregion

        #endregion

        #region Result<T> Signatures

        #region 400

        public static bool Has400Error<T>(this Result<T> result)
        {
            return _has400Error(result);
        }

        public static void Set400Error<T>(this Result<T> result)
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