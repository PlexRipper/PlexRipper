namespace Reaparr.Data.Contracts;

public interface IReaparrDbContextFactory
{
    Task<IReaparrDbContext> CreateAsync();
    IReaparrDbContext Create();
}