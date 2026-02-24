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
        if (request is not null)
        {
            log.Verbose(
                "{Method}: {EndpointPath} with {Request}",
                context.Request.Method,
                context.Request.GetDisplayUrl(),
                request
            );
            return;
        }

        log.Verbose("{Method}: {EndpointPath}", context.Request.Method, context.Request.GetDisplayUrl());
    }

    public static void DebugApiCall(this ILogger log, HttpContext context, object? request = null)
    {
        if (request is not null)
        {
            log.Debug(
                "{Method}: {EndpointPath} with {Request}",
                context.Request.Method,
                context.Request.GetDisplayUrl(),
                request
            );
            return;
        }

        log.Debug("{Method}: {EndpointPath}", context.Request.Method, context.Request.GetDisplayUrl());
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static string DebugMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Debug(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }
}
