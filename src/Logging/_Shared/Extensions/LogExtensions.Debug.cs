using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Core;

namespace Reaparr.Logging;

public static partial class LogExtensions
{
    private static string GetDisplayUrl(this HttpRequest request) => $"{request.Scheme}://{request.Host}{request.Path}";

    public static void VerboseApiCall(this ILogger log, HttpContext context, object? request = null)
    {
        if (request is not null)
        {
            log.Verbose(
                "{Method}: {EndpointPath} with {@Request} {Query}",
                context.Request.Method,
                context.Request.GetDisplayUrl(),
                request,
                context.Request.Query
            );
            return;
        }

        log.Verbose(
            "{Method}: {EndpointPath} {Query}",
            context.Request.Method,
            context.Request.GetDisplayUrl(),
            context.Request.Query
        );
    }

    public static void DebugApiCall(this ILogger log, HttpContext context, object? request = null)
    {
        if (request is not null)
        {
            log.Debug(
                "{Method}: {EndpointPath} with {@Request}",
                context.Request.Method,
                context.Request.GetDisplayUrl(),
                request
            );
            return;
        }

        log.Debug(
            "{Method}: {EndpointPath} {Query}",
            context.Request.Method,
            context.Request.GetDisplayUrl(),
            context.Request.Query
        );
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static string DebugMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Debug(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }
}
