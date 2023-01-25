#nullable enable
using Serilog.Context;
using Serilog.Events;

namespace Logging.LogStatic;

public static partial class LogStatic
{
    private static void Write(
        LogEventLevel logLevel,
        string messageTemplate,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0,
        params object?[]? propertyValues)
    {
        var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);

        using (LogContext.PushProperty("FileName", fileName))
        using (LogContext.PushProperty("MemberName", memberName))
        using (LogContext.PushProperty("LineNumber", sourceLineNumber))
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            Serilog.Log.Write(logLevel, messageTemplate, propertyValues);
        }
    }

    private static void Write(
        LogEventLevel logLevel,
        string messageTemplate,
        Exception? exception = default,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0,
        params object?[]? propertyValues)
    {
        var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);

        using (LogContext.PushProperty("FileName", fileName))
        using (LogContext.PushProperty("MemberName", memberName))
        using (LogContext.PushProperty("LineNumber", sourceLineNumber))
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            Serilog.Log.Write(logLevel, exception, messageTemplate, propertyValues);
        }
    }
}