using System;
using System.Linq;
using System.Runtime.CompilerServices;
using FluentResults;

namespace PlexRipper.Domain
{
    public static partial class ResultExtensions
    {
        #region Implementations

        private static Result _LogWarning(this Result result, string memberName = "", string sourceFilePath = "")
        {
            foreach (var error in result.Errors)
            {
                Log.Warning(error.Message, memberName, sourceFilePath);
                if (error.Reasons.Any())
                {
                    error.Reasons.ForEach(x => Log.Warning(x.Message, memberName, sourceFilePath));
                }
            }
            return result;
        }

        private static Result _LogError(this Result result, Exception e = null, string memberName = "", string sourceFilePath = "")
        {
            foreach (var error in result.Errors)
            {
                Log.Error(error.Message, memberName, sourceFilePath);
                if (error.Reasons.Any())
                {
                    error.Reasons.ForEach(x => Log.Error(x.Message, memberName, sourceFilePath));
                }
            }

            if (e != null)
            {
                Log.Error(e, memberName, sourceFilePath);
            }

            return result;
        }

        #endregion

        #region Result Signatures

        public static Result LogWarning(this Result result, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return _LogWarning(result, memberName, sourceFilePath);
        }

        public static Result LogError(this Result result, Exception e = null, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return _LogError(result, e, memberName, sourceFilePath);
        }

        #endregion


        #region Result<T> Signatures

        public static Result<T> LogWarning<T>(this Result<T> result, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return _LogWarning(result, memberName, sourceFilePath);
        }

        public static Result<T> LogError<T>(this Result<T> result, Exception e = null, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return _LogError(result, e, memberName, sourceFilePath);
        }

        #endregion
    }
}