using Microsoft.EntityFrameworkCore.Design;
using Reaparr.Environment;

namespace Reaparr.Data;

public class ReaparrDbDesignTimeContext : IDesignTimeDbContextFactory<ReaparrDbContext>
{
    public ReaparrDbContext CreateDbContext(string[] args) => new(PathProvider.DatabaseName);
}
