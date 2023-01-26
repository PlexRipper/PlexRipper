using System.Runtime.CompilerServices;
using Serilog.Core;
using Serilog.Events;

namespace Logging.LogStatic;

public static partial class LogStatic
{
    #region Error

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void ErrorLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Error, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Error(
        Exception ex,
        string messageTemplate = "Exception",
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Error, messageTemplate, ex, memberName!, sourceFilePath!, sourceLineNumber);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Error<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Error, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber, propertyValue);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Error<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Error, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber, propertyValue0, propertyValue1);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Error<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Error, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber, propertyValue0, propertyValue1, propertyValue2);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Error<T0, T1, T2, T3>(
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
        Write(LogEventLevel.Error, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber, propertyValue0, propertyValue1, propertyValue2,
            propertyValue3);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Error<T0, T1, T2, T3, T4>(
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
        Write(LogEventLevel.Error, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber, propertyValue0, propertyValue1, propertyValue2,
            propertyValue3, propertyValue4);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Error<T0, T1, T2, T3, T4, T5>(
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
        Write(LogEventLevel.Error, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber, propertyValue0, propertyValue1, propertyValue2,
            propertyValue3, propertyValue4, propertyValue5);
    }

    #endregion
}