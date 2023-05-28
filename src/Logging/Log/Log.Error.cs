using System.Runtime.CompilerServices;
using Logging.Common;
using Logging.Interface;
using Serilog.Core;
using Serilog.Events;

namespace Logging;

public partial class Log : ILog
{
    #region Error

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData ErrorLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        return Write(LogEventLevel.Error, messageTemplate, sourceFilePath, memberName, sourceLineNumber);
    }

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Error(
        Exception ex,
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        return Write(LogEventLevel.Error, ex, messageTemplate, sourceFilePath, memberName, sourceLineNumber);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Error<T>(
        Exception ex,
        string messageTemplate,
        T propertyValue = default,
        string memberName = default,
        string sourceFilePath = default,
        int sourceLineNumber = default)
    {
        return Write(LogEventLevel.Error, ex, messageTemplate, sourceFilePath, memberName, sourceLineNumber, propertyValue);
    }

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Error(Exception ex, string memberName = default, string sourceFilePath = default, int sourceLineNumber = default)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        return Write(LogEventLevel.Error, ex, "Exception", sourceFilePath, memberName, sourceLineNumber);
    }

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Error<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        return Write(LogEventLevel.Error, messageTemplate, sourceFilePath, memberName, sourceLineNumber, propertyValue);
    }

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Error<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        return Write(LogEventLevel.Error, messageTemplate, sourceFilePath, memberName, sourceLineNumber, propertyValue0, propertyValue1);
    }

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Error<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        return Write(LogEventLevel.Error, messageTemplate, sourceFilePath, memberName, sourceLineNumber, propertyValue0, propertyValue1,
            propertyValue2);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Error<T0, T1, T2, T3>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        return Write(LogEventLevel.Error, messageTemplate, sourceFilePath, memberName, sourceLineNumber, propertyValue0, propertyValue1,
            propertyValue2,
            propertyValue3);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Error<T0, T1, T2, T3, T4>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        return Write(LogEventLevel.Error, messageTemplate, sourceFilePath, memberName, sourceLineNumber, propertyValue0, propertyValue1,
            propertyValue2,
            propertyValue3, propertyValue4);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Error<T0, T1, T2, T3, T4, T5>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        T5 propertyValue5,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        return Write(LogEventLevel.Error, messageTemplate, sourceFilePath, memberName, sourceLineNumber, propertyValue0, propertyValue1,
            propertyValue2,
            propertyValue3, propertyValue4, propertyValue5);
    }

    #endregion
}