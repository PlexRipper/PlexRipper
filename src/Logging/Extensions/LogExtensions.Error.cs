using FluentResults;
using Serilog;
using Serilog.Core;

namespace Reaparr.Logging;

public static partial class LogExtensions
{
    public static Result ErrorResult(this ILogger log, Exception? ex)
    {
        log.ErrorResult(ex, "");

        return Result.Fail(new ExceptionalError(ex));
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static Result ErrorResult(this ILogger log, Exception? ex, string messageTemplate, params object[] args)
    {
        log.Error(ex, messageTemplate, args);

        var renderedMessage = log.RenderMessage(messageTemplate, args);

        return Result.Fail(new ExceptionalError(renderedMessage, ex));
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static Result ErrorResult(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Error(messageTemplate, args);

        var renderedMessage = log.RenderMessage(messageTemplate, args);

        return Result.Fail(new Error(renderedMessage));
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static string ErrorMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Error(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }
}