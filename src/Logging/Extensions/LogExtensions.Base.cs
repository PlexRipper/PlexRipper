using System.Runtime.CompilerServices;
using FluentResults;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace Reaparr.Logging;

public static partial class LogExtensions
{
    private static readonly MessageTemplateParser _templateParser = new();

    public static bool IsLogLevelEnabled(this ILogger logger, LogEventLevel logLevel = LogEventLevel.Debug) =>
        logger.IsEnabled(logLevel);

    public static bool IsLogLevelVerbose(this ILogger logger) => logger.IsEnabled(LogEventLevel.Verbose);

    public static bool IsLogLevelDebug(this ILogger logger) => logger.IsEnabled(LogEventLevel.Debug);

    public static ILogger Here(
        this ILogger logger,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => logger
        .ForContext(nameof(LogMetaData.ClassName), Path.GetFileNameWithoutExtension(sourceFilePath))
        .ForContext(nameof(LogMetaData.MethodName), memberName)
        .ForContext(nameof(LogMetaData.LineNumber), sourceLineNumber);




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