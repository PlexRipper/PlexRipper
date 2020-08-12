using FluentResults;

namespace PlexRipper.Domain.FluentResultExtensions
{
    public static class FluentResultExtensions
    {
        private static string _statusCode = "StatusCode";

        private static bool FindStatusCode(Result result, int statusCode)
        {
            foreach (Error error in result.Errors)
            {
                foreach (var (key, metaData) in error.Metadata)
                {
                    if (key == _statusCode && (int)metaData == statusCode)
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