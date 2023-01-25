using System.Runtime.CompilerServices;
using Serilog.Core;
using Serilog.Events;

namespace Logging.LogStatic;

public static partial class LogStatic
{
    #region Debug

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void DebugLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Debug, messageTemplate, memberName, sourceFilePath, sourceLineNumber);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Debug(
        Exception ex,
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Debug, messageTemplate, ex, memberName, sourceFilePath, sourceLineNumber);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Debug<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Debug, messageTemplate, memberName, sourceFilePath, sourceLineNumber, propertyValue);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Debug<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Debug, messageTemplate, memberName, sourceFilePath, sourceLineNumber, propertyValue0, propertyValue1);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Debug<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Debug, messageTemplate, memberName, sourceFilePath, sourceLineNumber, propertyValue0, propertyValue1, propertyValue2);
    }

    #endregion
}