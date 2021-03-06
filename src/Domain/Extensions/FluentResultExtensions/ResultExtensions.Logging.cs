﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FluentResults;
using Serilog.Events;

namespace PlexRipper.Domain
{
    /// <summary>
    /// Part of the <see cref="ResultExtensions"/> class - Logging related functionality.
    /// </summary>
    public static partial class ResultExtensions
    {
        #region Result Signatures

        /// <summary>
        /// Logs all nested reasons and metadata on Log.Verbose().
        /// </summary>
        /// <param name="result">The result to use for logging.</param>
        /// <param name="memberName">The function name where the result happened.</param>
        /// <param name="sourceFilePath">The path to the source.</param>
        /// <returns>The result unchanged.</returns>
        public static Result LogVerbose(
            this Result result,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return LogResult(result, LogEventLevel.Verbose, null, memberName, sourceFilePath);
        }

        /// <summary>
        /// Logs all nested reasons and metadata on Log.Debug().
        /// </summary>
        /// <param name="result">The result to use for logging.</param>
        /// <param name="memberName">The function name where the result happened.</param>
        /// <param name="sourceFilePath">The path to the source.</param>
        /// <returns>The result unchanged.</returns>
        public static Result LogDebug(
            this Result result,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return LogResult(result, LogEventLevel.Debug, null, memberName, sourceFilePath);
        }

        /// <summary>
        /// Logs all nested reasons and metadata on Log.Information().
        /// </summary>
        /// <param name="result">The result to use for logging.</param>
        /// <param name="memberName">The function name where the result happened.</param>
        /// <param name="sourceFilePath">The path to the source.</param>
        /// <returns>The result unchanged.</returns>
        public static Result LogInformation(
            this Result result,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return LogResult(result, LogEventLevel.Information, null, memberName, sourceFilePath);
        }

        /// <summary>
        /// Logs all nested reasons and metadata on Log.Warning().
        /// </summary>
        /// <param name="result">The result to use for logging.</param>
        /// <param name="memberName">The function name where the result happened.</param>
        /// <param name="sourceFilePath">The path to the source.</param>
        /// <returns>The result unchanged.</returns>
        public static Result LogWarning(this Result result, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
        {
            return LogResult(result, LogEventLevel.Warning, null, memberName, sourceFilePath);
        }

        /// <summary>
        /// Logs all nested reasons and metadata on Log.Error().
        /// </summary>
        /// <param name="result">The result to use for logging.</param>
        /// <param name="e">The optional exception which can be passed to Log.Error().</param>
        /// <param name="memberName">The function name where the result happened.</param>
        /// <param name="sourceFilePath">The path to the source.</param>
        /// <returns>The result unchanged.</returns>
        public static Result LogError(
            this Result result,
            Exception e = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return LogResult(result, LogEventLevel.Error, e, memberName, sourceFilePath);
        }

        #endregion

        #region Result<T> Signatures

        /// <summary>
        /// Logs all nested reasons and metadata on Log.Verbose().
        /// </summary>
        /// <param name="result">The result to use for logging.</param>
        /// <param name="memberName">The function name where the result happened.</param>
        /// <param name="sourceFilePath">The path to the source.</param>
        /// <typeparam name="T">The result type.</typeparam>
        /// <returns>The result unchanged.</returns>
        public static Result<T> LogVerbose<T>(
            this Result<T> result,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return LogResult(result, LogEventLevel.Verbose, null, memberName, sourceFilePath);
        }

        /// <summary>
        /// Logs all nested reasons and metadata on Log.Debug().
        /// </summary>
        /// <param name="result">The result to use for logging.</param>
        /// <param name="memberName">The function name where the result happened.</param>
        /// <param name="sourceFilePath">The path to the source.</param>
        /// <typeparam name="T">The result type.</typeparam>
        /// <returns>The result unchanged.</returns>
        public static Result<T> LogDebug<T>(
            this Result<T> result,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return LogResult(result, LogEventLevel.Debug, null, memberName, sourceFilePath);
        }

        /// <summary>
        /// Logs all nested reasons and metadata on Log.Information().
        /// </summary>
        /// <param name="result">The result to use for logging.</param>
        /// <param name="memberName">The function name where the result happened.</param>
        /// <param name="sourceFilePath">The path to the source.</param>
        /// <typeparam name="T">The result type.</typeparam>
        /// <returns>The result unchanged.</returns>
        public static Result<T> LogInformation<T>(
            this Result<T> result,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return LogResult(result, LogEventLevel.Information, null, memberName, sourceFilePath);
        }

        /// <summary>
        /// Logs all nested reasons and metadata on Log.Warning().
        /// </summary>
        /// <param name="result">The result to use for logging.</param>
        /// <param name="memberName">The function name where the result happened.</param>
        /// <param name="sourceFilePath">The path to the source.</param>
        /// <typeparam name="T">The result type.</typeparam>
        /// <returns>The result unchanged.</returns>
        public static Result<T> LogWarning<T>(
            this Result<T> result,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return LogResult(result, LogEventLevel.Warning, null, memberName, sourceFilePath);
        }

        /// <summary>
        /// Logs all nested reasons and metadata on Log.Error().
        /// </summary>
        /// <param name="result">The result to use for logging.</param>
        /// <param name="e">The optional exception which can be passed to Log.Error().</param>
        /// <param name="memberName">The function name where the result happened.</param>
        /// <param name="sourceFilePath">The path to the source.</param>
        /// <typeparam name="T">The result type.</typeparam>
        /// <returns>The result unchanged.</returns>
        public static Result<T> LogError<T>(
            this Result<T> result,
            Exception e = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            return LogResult(result, LogEventLevel.Error, e, memberName, sourceFilePath);
        }

        #endregion

        #region Implementations

        private static void LogByType(LogEventLevel logLevel, string message, Exception e = null, string memberName = "", string sourceFilePath = "")
        {
            switch (logLevel)
            {
                case LogEventLevel.Verbose:
                    Log.Verbose(message, e, memberName, sourceFilePath);
                    break;
                case LogEventLevel.Debug:
                    Log.Debug(message, e, memberName, sourceFilePath);
                    break;
                case LogEventLevel.Information:
                    Log.Information(message, e, memberName, sourceFilePath);
                    break;
                case LogEventLevel.Warning:
                    Log.Warning(message, e, memberName, sourceFilePath);
                    break;
                case LogEventLevel.Error:
                    Log.Error(message, e, memberName, sourceFilePath);
                    break;
                case LogEventLevel.Fatal:
                    Log.Fatal(message, e, memberName, sourceFilePath);
                    break;
            }
        }

        private static Result<T> LogResult<T>(
            this Result<T> result,
            LogEventLevel logLevel,
            Exception e = null,
            string memberName = "",
            string sourceFilePath = "")
        {
            LogReasons(result, logLevel, e, memberName, sourceFilePath);

            return result;
        }

        private static Result LogResult(
            this Result result,
            LogEventLevel logLevel,
            Exception e = null,
            string memberName = "",
            string sourceFilePath = "")
        {
            LogReasons(result, logLevel, e, memberName, sourceFilePath);

            return result;
        }

        private static void LogReasons(
            this Result result,
            LogEventLevel logLevel,
            Exception e = null,
            string memberName = "",
            string sourceFilePath = "")
        {
            foreach (var reason in result.Reasons)
            {
                LogByType(logLevel, reason.Message, e, memberName, sourceFilePath);

                if (reason.Metadata.Any())
                {
                    foreach (KeyValuePair<string, object> entry in reason.Metadata)
                    {
                        LogByType(logLevel, $"{entry.Key} - {entry.Value}", null, memberName, sourceFilePath);
                    }
                }
            }

            if (e != null)
            {
                LogByType(logLevel, string.Empty, e, memberName, sourceFilePath);
            }
        }

        #endregion
    }
}