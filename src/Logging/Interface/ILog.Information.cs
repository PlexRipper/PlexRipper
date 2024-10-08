using System.Runtime.CompilerServices;
using Logging.Common;
using Serilog.Core;
using Serilog.Events;

namespace Logging.Interface;

public partial interface ILog
{
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
    LogMetaData InformationLine(
        string messageTemplate,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );

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
    LogMetaData Information<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );

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
    LogMetaData Information<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );

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
    LogMetaData Information<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );

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
    LogMetaData Information(
        Exception? exception,
        string messageTemplate,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Information<T0, T1, T2, T3>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Information<T0, T1, T2, T3, T4>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );

    #endregion
}