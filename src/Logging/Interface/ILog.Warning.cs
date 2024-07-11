using System.Runtime.CompilerServices;
using Logging.Common;
using Serilog.Core;

namespace Logging.Interface;

public partial interface ILog
{
    #region Warning

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData WarningLine(
        string messageTemplate,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );

    #region Exception

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Warning(
        Exception ex,
        string messageTemplate,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );

    #endregion

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Warning<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Warning<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Warning<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Warning<T0, T1, T2, T3>(
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
    LogMetaData Warning<T0, T1, T2, T3, T4>(
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

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Warning<T0, T1, T2, T3, T4, T5>(
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
    );

    #endregion
}
