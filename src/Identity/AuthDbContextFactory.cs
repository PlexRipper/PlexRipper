namespace Reaparr.Identity;

public class AuthDbContextFactory : IAuthDbContextFactory
{
    private readonly Func<IAuthDbContext> _factory;

    /// <summary>
    /// Uses Autofac's Func&lt;T&gt; factory delegation pattern.
    /// Autofac automatically provides Func&lt;T&gt; for any registered type.
    /// This ensures test overrides of IAuthDbContext are respected.
    /// </summary>
    public AuthDbContextFactory(Func<IAuthDbContext> factory)
    {
        _factory = factory;
    }

    public IAuthDbContext Create() => _factory();

    public Task<IAuthDbContext> CreateAsync() => Task.FromResult(_factory());
}
