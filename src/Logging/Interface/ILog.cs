using System.Runtime.CompilerServices;
using Serilog.Core;
using Serilog.Events;

namespace Logging.Interface;

public interface ILog
{
    #region Verbose

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent VerboseLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Verbose(
        Exception ex,
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Verbose<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Verbose<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Verbose<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Verbose<T0, T1, T2, T3>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Verbose<T0, T1, T2, T3, T4>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Verbose<T0, T1, T2, T3, T4, T5>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        T5 propertyValue5,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    #endregion

    #region Debug

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="memberName">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceFilePath">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <example><code>
    /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
    /// </code></example>
    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent DebugLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="memberName">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceFilePath">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <example>
    /// Log.Debug(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Debug(
        Exception exception,
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="memberName">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceFilePath">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Debug<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="memberName">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceFilePath">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <example>
    /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Debug<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <param name="memberName">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceFilePath">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// /// <example>
    /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Debug<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Debug<T0, T1, T2, T3>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Debug<T0, T1, T2, T3, T4>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Debug<T0, T1, T2, T3, T4, T5>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        T5 propertyValue5,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    #endregion

    #region Information

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="memberName">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceFilePath">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <example><code>
    /// Log.Information("Starting up at {StartedAt}.", DateTime.Now);
    /// </code></example>
    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent InformationLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="memberName">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceFilePath">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Information<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="memberName">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceFilePath">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <example>
    /// Log.Information("Starting up at {StartedAt}.", DateTime.Now);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Information<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <param name="memberName">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceFilePath">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <example>
    /// Log.Information("Starting up at {StartedAt}.", DateTime.Now);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Information<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="memberName">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceFilePath">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <param name="sourceLineNumber">This is automatically passed by the Caller Information and should not be filled in.</param>
    /// <example>
    /// Log.Information(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Information(
        Exception exception,
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Information<T0, T1, T2, T3>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Information<T0, T1, T2, T3, T4>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    #endregion

    #region Warning

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent WarningLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Warning<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Warning<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Warning<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Warning<T0, T1, T2, T3>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Warning<T0, T1, T2, T3, T4>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Warning<T0, T1, T2, T3, T4, T5>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        T5 propertyValue5,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    #endregion

    #region Error

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent ErrorLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    #region Exception

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Error(
        Exception ex,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Error(
        Exception ex,
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    #endregion

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Error<T>(
        string messageTemplate,
        T propertyValue,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Error<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Error<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Error<T0, T1, T2, T3>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Error<T0, T1, T2, T3, T4>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Error<T0, T1, T2, T3, T4, T5>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        T5 propertyValue5,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    #endregion

    #region Fatal

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent FatalLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Fatal(
        Exception ex,
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Fatal<T>(
        Exception ex,
        string messageTemplate,
        T propertyValue = default,
        string memberName = default,
        string sourceFilePath = default,
        int sourceLineNumber = default);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Fatal(Exception ex, string memberName = default, string sourceFilePath = default, int sourceLineNumber = default);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Fatal<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Fatal<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Fatal<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Fatal<T0, T1, T2, T3>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Fatal<T0, T1, T2, T3, T4>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogEvent Fatal<T0, T1, T2, T3, T4, T5>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        T5 propertyValue5,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    #endregion

    bool IsLogLevelEnabled(LogEventLevel logLevel = LogEventLevel.Debug);
}

public interface ILog<T> : ILog where T : class { }