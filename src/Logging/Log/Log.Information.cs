using System.Runtime.CompilerServices;
using Logging.Common;
using Serilog.Core;
using Serilog.Events;

namespace Logging;

public partial class Log
{
    #region Information

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData InformationLine(
        string messageTemplate,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => Write(LogEventLevel.Information, messageTemplate, sourceFilePath, memberName, sourceLineNumber);

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Information(
        Exception ex,
        string messageTemplate,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => Write(LogEventLevel.Information, ex, messageTemplate, sourceFilePath, memberName, sourceLineNumber);

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Information<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => Write(LogEventLevel.Information, messageTemplate, sourceFilePath, memberName, sourceLineNumber, propertyValue);

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Information<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) =>
        Write(
            LogEventLevel.Information,
            messageTemplate,
            sourceFilePath,
            memberName,
            sourceLineNumber,
            propertyValue0,
            propertyValue1
        );

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Information<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) =>
        Write(
            LogEventLevel.Information,
            messageTemplate,
            sourceFilePath,
            memberName,
            sourceLineNumber,
            propertyValue0,
            propertyValue1,
            propertyValue2
        );

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Information<T0, T1, T2, T3>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) =>
        Write(
            LogEventLevel.Information,
            messageTemplate,
            sourceFilePath,
            memberName,
            sourceLineNumber,
            propertyValue0,
            propertyValue1,
            propertyValue2,
            propertyValue3
        );

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Information<T0, T1, T2, T3, T4>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) =>
        Write(
            LogEventLevel.Information,
            messageTemplate,
            sourceFilePath,
            memberName,
            sourceLineNumber,
            propertyValue0,
            propertyValue1,
            propertyValue2,
            propertyValue3,
            propertyValue4
        );

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Information<T0, T1, T2, T3, T4, T5>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        T5 propertyValue5,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) =>
        Write(
            LogEventLevel.Information,
            messageTemplate,
            sourceFilePath,
            memberName,
            sourceLineNumber,
            propertyValue0,
            propertyValue1,
            propertyValue2,
            propertyValue3,
            propertyValue4,
            propertyValue5
        );

    #endregion
}
