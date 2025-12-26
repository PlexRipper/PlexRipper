using Reaparr.Data.Contracts;

namespace Reaparr.Data;

public class ReaparrDbContextFactory : IReaparrDbContextFactory
{
    private readonly Func<IReaparrDbContext> _factory;

    /// <summary>
    /// Uses Autofac's Func&lt;T&gt; factory delegation pattern.
    /// Autofac automatically provides Func&lt;T&gt; for any registered type.
    /// This ensures test overrides of IReaparrDbContext are respected.
    /// </summary>
    public ReaparrDbContextFactory(Func<IReaparrDbContext> factory)
    {
        _factory = factory;
    }

    public IReaparrDbContext Create() => _factory();

    public Task<IReaparrDbContext> CreateAsync() => Task.FromResult(_factory());
}
