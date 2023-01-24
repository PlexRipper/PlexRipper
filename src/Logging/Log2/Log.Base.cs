using Logging.Interface;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace Logging.Log2;

public partial class Log : ILog
{
    private readonly ILogger _logger;

    public Log(ILogger logger)
    {
        _logger = logger;
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    private void Write(
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
            _logger.Write(logLevel, messageTemplate, propertyValues);
        }
    }

    private void Write(
        LogEventLevel logLevel,
        string messageTemplate,
        Exception exception = default,
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
            _logger.Write(logLevel, exception, messageTemplate, propertyValues);
        }
    }
}