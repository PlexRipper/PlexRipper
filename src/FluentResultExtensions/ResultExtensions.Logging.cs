using System.Runtime.CompilerServices;
using Serilog.Core;
using Serilog.Events;

// ReSharper disable once CheckNamespace
// Needs to be in the same namespace as the FluentResults package
namespace FluentResults;

/// <summary>
/// Part of the <see cref="ResultExtensions"/> class - Logging related functionality.
/// </summary>
public static partial class ResultExtensions
{
    #region Implementations

    [MessageTemplateFormatMethod("messageTemplate")]
    private static void LogByType(
        LogEventLevel logLevel,
        string messageTemplate,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0
    )
    {
        switch (logLevel)
        {
            case LogEventLevel.Verbose:
                _log.Verbose(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                return;
            case LogEventLevel.Debug:
                _log.Debug(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case LogEventLevel.Information:
                _log.Information(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case LogEventLevel.Warning:
                _log.Warning(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case LogEventLevel.Error:
                _log.Error(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case LogEventLevel.Fatal:
                _log.Fatal(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    private static void LogByType(
        LogEventLevel logLevel,
        string messageTemplate,
        Exception? e,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0
    )
    {
        switch (logLevel)
        {
            case LogEventLevel.Verbose:
                _log.Verbose(e, messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case LogEventLevel.Debug:
                _log.Debug(e, messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case LogEventLevel.Information:
                _log.Information(e, messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case LogEventLevel.Warning:
                _log.Warning(e, messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case LogEventLevel.Error:
                _log.Error(e, messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case LogEventLevel.Fatal:
                _log.Fatal(e, messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }

    private static Result<T> LogResult<T>(
        this Result<T> result,
        LogEventLevel logLevel,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0
    )
    {
        LogReasons(result.ToResult(), logLevel, memberName, sourceFilePath, sourceLineNumber);

        return result;
    }

    private static Result LogResult(
        this Result result,
        LogEventLevel logLevel,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0
    )
    {
        LogReasons(result, logLevel, memberName, sourceFilePath, sourceLineNumber);

        return result;
    }

    private static void LogReasons(
        this Result result,
        LogEventLevel logLevel,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0
    )
    {
        foreach (var success in result.Successes)
            LogByType(logLevel, success.ToLogString(), null, memberName, sourceFilePath, sourceLineNumber);

        foreach (var error in result.Errors)
        {
            if (error is ExceptionalError exceptional)
            {
                var exception = exceptional.Exception;
                LogByType(logLevel, error.ToLogString(), exception, memberName, sourceFilePath, sourceLineNumber);
                continue;
            }

            LogByType(logLevel, error.ToLogString(), null, memberName, sourceFilePath, sourceLineNumber);
        }
    }

    private static string ToLogString(this IError error)
    {
        var msg = ((IReason)error).ToLogString();
        foreach (var reason in error.Reasons)
            msg += $"{System.Environment.NewLine} {Delimiter(1)} {reason.ToLogString(2)}";

        return msg;
    }

    private static string ToLogString(this IReason reason, int level = 1)
    {
        var msg = reason.Message;

        if (reason.Metadata.Any())
        {
            msg += $"{System.Environment.NewLine} {Delimiter(level)} Metadata:";
            foreach (var entry in reason.Metadata)
                msg += $"{System.Environment.NewLine} {Delimiter(level + 1)} {entry.Key} - {entry.Value}";
        }

        return msg;
    }

    private static string Delimiter(int level)
    {
        var delimiter = string.Empty;
        for (var i = 0; i < level; i++)
            delimiter += "-";

        return delimiter;
    }

    #endregion

    #region Result Signatures

    /// <summary>
    /// Logs all nested reasons and metadata on Log.Verbose().
    /// </summary>
    /// <param name="result">The result to use for logging.</param>
    /// <param name="memberName">The function name where the result happened.</param>
    /// <param name="sourceFilePath">The path to the source.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <returns>The result unchanged.</returns>
    public static Result LogVerbose(
        this Result result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => LogResult(result, LogEventLevel.Verbose, memberName, sourceFilePath, sourceLineNumber);

    /// <summary>
    /// Logs all nested reasons and metadata on Log.Debug().
    /// </summary>
    /// <param name="result">The result to use for logging.</param>
    /// <param name="memberName">The function name where the result happened.</param>
    /// <param name="sourceFilePath">The path to the source.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <returns>The result unchanged.</returns>
    public static Result LogDebug(
        this Result result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => LogResult(result, LogEventLevel.Debug, memberName, sourceFilePath, sourceLineNumber);

    /// <summary>
    /// Logs all nested reasons and metadata on Log.Information().
    /// </summary>
    /// <param name="result">The result to use for logging.</param>
    /// <param name="memberName">The function name where the result happened.</param>
    /// <param name="sourceFilePath">The path to the source.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <returns>The result unchanged.</returns>
    public static Result LogInformation(
        this Result result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => LogResult(result, LogEventLevel.Information, memberName, sourceFilePath, sourceLineNumber);

    /// <summary>
    /// Logs all nested reasons and metadata on Log.Warning().
    /// </summary>
    /// <param name="result">The result to use for logging.</param>
    /// <param name="memberName">The function name where the result happened.</param>
    /// <param name="sourceFilePath">The path to the source.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <returns>The result unchanged.</returns>
    public static Result LogWarning(
        this Result result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => LogResult(result, LogEventLevel.Warning, memberName, sourceFilePath, sourceLineNumber);

    /// <summary>
    /// Logs all nested reasons and metadata on Log.Error().
    /// </summary>
    /// <param name="result">The result to use for logging.</param>
    /// <param name="memberName">The function name where the result happened.</param>
    /// <param name="sourceFilePath">The path to the source.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <returns>The result unchanged.</returns>
    public static Result LogError(
        this Result result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => LogResult(result, LogEventLevel.Error, memberName, sourceFilePath, sourceLineNumber);

    /// <summary>
    /// Logs all nested reasons and metadata on Log.Fatal().
    /// </summary>
    /// <param name="result">The result to use for logging.</param>
    /// <param name="memberName">The function name where the result happened.</param>
    /// <param name="sourceFilePath">The path to the source.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <returns>The result unchanged.</returns>
    public static Result LogFatal(
        this Result result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => LogResult(result, LogEventLevel.Fatal, memberName, sourceFilePath, sourceLineNumber);

    #endregion

    #region Result<T> Signatures

    /// <summary>
    /// Logs all nested reasons and metadata on Log.Verbose().
    /// </summary>
    /// <param name="result">The result to use for logging.</param>
    /// <param name="memberName">The function name where the result happened.</param>
    /// <param name="sourceFilePath">The path to the source.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <typeparam name="T">The result type.</typeparam>
    /// <returns>The result unchanged.</returns>
    public static Result<T> LogVerbose<T>(
        this Result<T> result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => LogResult(result, LogEventLevel.Verbose, memberName, sourceFilePath, sourceLineNumber);

    /// <summary>
    /// Logs all nested reasons and metadata on Log.Debug().
    /// </summary>
    /// <param name="result">The result to use for logging.</param>
    /// <param name="memberName">The function name where the result happened.</param>
    /// <param name="sourceFilePath">The path to the source.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <typeparam name="T">The result type.</typeparam>
    /// <returns>The result unchanged.</returns>
    public static Result<T> LogDebug<T>(
        this Result<T> result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => LogResult(result, LogEventLevel.Debug, memberName, sourceFilePath, sourceLineNumber);

    /// <summary>
    /// Logs all nested reasons and metadata on Log.Information().
    /// </summary>
    /// <param name="result">The result to use for logging.</param>
    /// <param name="memberName">The function name where the result happened.</param>
    /// <param name="sourceFilePath">The path to the source.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <typeparam name="T">The result type.</typeparam>
    /// <returns>The result unchanged.</returns>
    public static Result<T> LogInformation<T>(
        this Result<T> result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => LogResult(result, LogEventLevel.Information, memberName, sourceFilePath, sourceLineNumber);

    /// <summary>
    /// Logs all nested reasons and metadata on Log.Warning().
    /// </summary>
    /// <param name="result">The result to use for logging.</param>
    /// <param name="memberName">The function name where the result happened.</param>
    /// <param name="sourceFilePath">The path to the source.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <typeparam name="T">The result type.</typeparam>
    /// <returns>The result unchanged.</returns>
    public static Result<T> LogWarning<T>(
        this Result<T> result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => LogResult(result, LogEventLevel.Warning, memberName, sourceFilePath, sourceLineNumber);

    /// <summary>
    /// Logs all nested reasons and metadata on Log.Error().
    /// </summary>
    /// <param name="result">The result to use for logging.</param>
    /// <param name="memberName">The function name where the result happened.</param>
    /// <param name="sourceFilePath">The path to the source.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <typeparam name="T">The result type.</typeparam>
    /// <returns>The result unchanged.</returns>
    public static Result LogError<T>(
        this Result<T> result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => LogResult(result, LogEventLevel.Error, memberName, sourceFilePath, sourceLineNumber).ToResult();

    /// <summary>
    /// Logs all nested reasons and metadata on Log.Fatal().
    /// </summary>
    /// <param name="result">The result to use for logging.</param>
    /// <param name="memberName">The function name where the result happened.</param>
    /// <param name="sourceFilePath">The path to the source.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <typeparam name="T">The result type.</typeparam>
    /// <returns>The result unchanged.</returns>
    public static Result LogFatal<T>(
        this Result<T> result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => LogResult(result, LogEventLevel.Fatal, memberName, sourceFilePath, sourceLineNumber).ToResult();

    #endregion
}
