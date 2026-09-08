namespace Reaparr.PublicAPI;

public class DownloadClientAuthenticationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly ILogger _log;

    public DownloadClientAuthenticationPreProcessor(ILogger log, IReaparrDbContextFactory dbContextFactory)
    {
        _log = log.ForContext<DownloadClientAuthenticationPreProcessor<TRequest>>();
        _dbContextFactory = dbContextFactory;
    }

    public async Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();
        var identity = await ctx.HttpContext.AuthenticateBearerAsync(dbContext, ct);
        if (identity is not null)
            return;

        _log.Here()
            .Warning(
                "Invalid or missing integration download-client Bearer key from {UserAgent} for request to '{RequestPath}'",
                ctx.HttpContext.Request.Headers.UserAgent.ToString(),
                ctx.HttpContext.Request.Path
            );
        await ctx.HttpContext.Response.SendUnauthorizedAsync(ct);
    }
}
