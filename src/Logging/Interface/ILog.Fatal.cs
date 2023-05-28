using System.Runtime.CompilerServices;
using Logging.Common;
using Serilog.Core;

namespace Logging.Interface;

public partial interface ILog
{
    #region Fatal

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData FatalLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Fatal(
        Exception ex,
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Fatal<T>(
        Exception ex,
        string messageTemplate,
        T propertyValue = default,
        string memberName = default,
        string sourceFilePath = default,
        int sourceLineNumber = default);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Fatal(Exception ex, string memberName = default, string sourceFilePath = default, int sourceLineNumber = default);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Fatal<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Fatal<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Fatal<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Fatal<T0, T1, T2, T3>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!);

    [MessageTemplateFormatMethod("messageTemplate")]
    LogMetaData Fatal<T0, T1, T2, T3, T4>(
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
    LogMetaData Fatal<T0, T1, T2, T3, T4, T5>(
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
}