namespace Reaparr.PublicAPI;

public class IndexerAuthenticationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly ILogger _log;

    public IndexerAuthenticationPreProcessor(ILogger log, IReaparrDbContextFactory dbContextFactory)
    {
        _log = log.ForContext<IndexerAuthenticationPreProcessor<TRequest>>();
        _dbContextFactory = dbContextFactory;
    }

    public async Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        var dbContext = await _dbContextFactory.CreateAsync();
        var identity = await ctx.HttpContext.AuthenticateQueryKeyAsync(dbContext, ct);
        if (identity is not null)
            return;

        _log.Here()
            .Warning(
                "Invalid or missing integration indexer API key from {UserAgent} for request to '{RequestPath}'",
                ctx.HttpContext.Request.Headers.UserAgent.ToString(),
                ctx.HttpContext.Request.Path
            );
        await ctx.HttpContext.Response.SendUnauthorizedAsync(ct);
    }
}
