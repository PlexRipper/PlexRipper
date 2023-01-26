using System.Runtime.CompilerServices;
using Serilog.Core;
using Serilog.Events;

namespace Logging.LogStatic;

public static partial class LogStatic
{
    #region Verbose

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void VerboseLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Verbose, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Verbose(
        Exception ex,
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Verbose, messageTemplate, ex, memberName!, sourceFilePath!, sourceLineNumber);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Verbose<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Verbose, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber, propertyValue);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Verbose<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Verbose, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber, propertyValue0, propertyValue1);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Verbose<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Verbose, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber, propertyValue0, propertyValue1, propertyValue2);
    }

    #endregion
}