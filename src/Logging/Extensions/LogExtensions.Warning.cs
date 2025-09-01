using Serilog;
using Serilog.Core;
using FluentResults;

namespace Reaparr.Logging;

public static partial class LogExtensions
{
    [MessageTemplateFormatMethod("messageTemplate")]
    public static Result WarningResult(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Warning(messageTemplate, args);

        var renderedMessage = log.RenderMessage(messageTemplate, args);

        return Result.Fail(new Error(renderedMessage));
    }
    [MessageTemplateFormatMethod("messageTemplate")]
    public static string WarningMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Warning(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }

}
