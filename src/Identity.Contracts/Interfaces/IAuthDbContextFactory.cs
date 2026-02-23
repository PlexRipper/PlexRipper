namespace Reaparr.Identity.Contracts;

public interface IAuthDbContextFactory
{
    Task<IAuthDbContext> CreateAsync();
    IAuthDbContext Create();
}
