namespace Reaparr.AppHost;

/// <summary>
/// Suppresses <see cref="OperationCanceledException"/> (and its derived <see cref="TaskCanceledException"/>)
/// that are caused by the client aborting an in-flight HTTP request.
/// TODO: See if this is already implemented: https://youtrack.jetbrains.com/issue/RIDER-135982/Add-setting-to-disable-breaking-on-BreakForUserUnhandledException-when-CLR-Exception-Breakpoints-are-enabled
/// WHY THIS EXISTS:
///   The frontend uses AbortController (via Axios + RxJS) to cancel requests when the user navigates
///   away or an observable is unsubscribed. The browser sends a TCP RST/FIN and ASP.NET Core sets
///   HttpContext.RequestAborted, which propagates a CancellationToken into every downstream layer
///   (FastEndpoints handlers, EF Core queries, etc.). Those layers correctly throw
///   OperationCanceledException — that is the intended, safe behavior.
///
/// WHY IT MUST NOT BE TREATED AS AN ERROR:
///   An aborted request is a normal part of the client-server contract, not an application fault.
///   Letting the exception propagate to DeveloperExceptionPageMiddleware causes Rider to break
///   execution on every routine navigation event, and would log false error noise in production.
///
/// SAFETY FILTER:
///   Only exceptions where HttpContext.RequestAborted.IsCancellationRequested is true are swallowed.
///   Any OperationCanceledException thrown by internal application logic (e.g. a real timeout,
///   a bug in a background task) will have a different CancellationToken and will NOT be caught here,
///   so real errors are still surfaced normally.
/// </summary>
public sealed class RequestCancellationMiddleware(RequestDelegate next)
{
    /// <summary>
    ///  Invokes the middleware to catch and suppress OperationCanceledException caused by client request cancellation.
    /// </summary>
    /// <param name="ctx"></param>
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected — this is expected. Return 499 (nginx convention for client-closed
            // request) so access logs reflect what happened without triggering error alerting.
            if (!ctx.Response.HasStarted)
                ctx.Response.StatusCode = 499;
        }
    }
}
