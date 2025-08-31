using System.Runtime.CompilerServices;
using FluentResults;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace Reaparr.Logging;

public static partial class LogExtensions
{
    public static bool IsLogLevelEnabled(this ILogger logger, LogEventLevel logLevel = LogEventLevel.Debug) =>
        logger.IsEnabled(logLevel);

    public static bool IsLogLevelVerbose(this ILogger logger) => logger.IsEnabled(LogEventLevel.Verbose);

    public static bool IsLogLevelDebug(this ILogger logger) => logger.IsEnabled(LogEventLevel.Debug);

    private static readonly MessageTemplateParser _templateParser = new();

    public static ILogger Here(
        this ILogger logger,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) =>
        logger
            .ForContext(nameof(LogMetaData.ClassName), Path.GetFileNameWithoutExtension(sourceFilePath))
            .ForContext(nameof(LogMetaData.MethodName), memberName)
            .ForContext(nameof(LogMetaData.LineNumber), sourceLineNumber);

    public static Result ErrorResult(this ILogger log, Exception? ex)
    {
        log.ErrorResult(ex, "");

        return Result.Fail(new ExceptionalError(ex));
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static Result ErrorResult(this ILogger log, Exception? ex, string messageTemplate, params object[] args)
    {
        log.ErrorResult(ex, messageTemplate, args);

        var renderedMessage = log.RenderMessage(messageTemplate, args);

        return Result.Fail(new ExceptionalError(renderedMessage, ex));
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static Result WarningResult(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Warning(messageTemplate, args);

        var renderedMessage = log.RenderMessage(messageTemplate, args);

        return Result.Fail(new Error(renderedMessage));
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static Result ErrorResult(this ILogger log, string messageTemplate, params object[] args)
    {
        log.ErrorResult(messageTemplate, args);

        var renderedMessage = log.RenderMessage(messageTemplate, args);

        return Result.Fail(new Error(renderedMessage));
    }

    public static string RenderMessage(this ILogger log, string messageTemplate, params object[] args)
    {
        var parsed = _templateParser.Parse(messageTemplate);

        var logEvent = new LogEvent(
            DateTimeOffset.Now,
            LogEventLevel.Information, // Level doesn’t matter for rendering
            exception: null,
            messageTemplate: parsed,
            properties: args.Select((a, i) => new LogEventProperty("Arg" + i, new ScalarValue(a)))
        );

        return parsed.Render(logEvent.Properties);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static string DebugMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Debug(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static string InformationMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Information(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static string WarningMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Warning(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static string ErrorMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Error(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }

    public static Result ToResult(this LogMetaData logMetaData)
    {
        var error = new Error(logMetaData.ToString());
        error.Metadata.Add("ClassName", logMetaData.ClassName);
        error.Metadata.Add("MethodName", logMetaData.MethodName);
        error.Metadata.Add("LineNumber", logMetaData.LineNumber);
        error.Metadata.Add("LogLevel", logMetaData.LogLevel);
        error.Metadata.Add("Exception", logMetaData.Exception);

        return Result.Fail(error);
    }
}
