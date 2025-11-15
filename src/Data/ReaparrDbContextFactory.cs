using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;

namespace Reaparr.Data;

public class ReaparrDbContextFactory : IReaparrDbContextFactory
{
    private readonly IDbContextFactory<ReaparrDbContext> _factory;

    public ReaparrDbContextFactory(IDbContextFactory<ReaparrDbContext> factory)
    {
        _factory = factory;
    }

    public IReaparrDbContext Create() => _factory.CreateDbContext();

    public async Task<IReaparrDbContext> CreateAsync() => await _factory.CreateDbContextAsync();
}