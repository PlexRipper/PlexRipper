using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Core;

namespace Reaparr.Logging;

public static partial class LogExtensions
{
    private static string GetDisplayUrl(this HttpRequest request) =>
        $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

    public static void VerboseApiCall(this ILogger log, HttpContext context, object? request = null)
    {
        log.Verbose(
            "{Method}: {EndpointPath} with {Request}",
            context.Request.Method,
            context.Request.GetDisplayUrl(),
            request
        );
    }

    public static void DebugApiCall(this ILogger log, HttpContext context, object? request = null)
    {
        log.Debug(
            "{Method}: {EndpointPath} with {Request}",
            context.Request.Method,
            context.Request.GetDisplayUrl(),
            request
        );
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static string DebugMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Debug(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }
}
