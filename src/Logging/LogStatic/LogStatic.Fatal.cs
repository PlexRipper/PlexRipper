using System.Runtime.CompilerServices;
using Serilog.Core;
using Serilog.Events;

namespace Logging.LogStatic;

public static partial class LogStatic
{
    #region Fatal

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void FatalLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Fatal, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber);
        FatalAction();
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Fatal(
        Exception ex,
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Fatal, messageTemplate, ex, memberName!, sourceFilePath!, sourceLineNumber);
        FatalAction();
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Fatal(
        Exception ex,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Fatal, "Exception:", ex, memberName!, sourceFilePath!, sourceLineNumber);
        FatalAction();
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Fatal<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Fatal, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber, propertyValue);
        FatalAction();
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Fatal<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Fatal, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber, propertyValue0, propertyValue1);
        FatalAction();
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Fatal<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Fatal, messageTemplate, memberName!, sourceFilePath!, sourceLineNumber, propertyValue0, propertyValue1, propertyValue2);
        FatalAction();
    }

    #endregion
}