namespace Reaparr.PublicAPI;

public class DownloadClientAuthenticationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public DownloadClientAuthenticationPreProcessor(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<DownloadClientAuthenticationPreProcessor<TRequest>>();
        _dbContext = dbContext;
    }

    public async Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        var identity = await ctx.HttpContext.AuthenticateBearerAsync(_dbContext, ct);
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
