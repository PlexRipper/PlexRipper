using Microsoft.EntityFrameworkCore;
using Reaparr.Domain;

namespace Reaparr.Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<PlexAccount> IncludeServerAccess(this IQueryable<PlexAccount> plexAccount) =>
        plexAccount.Include(v => v.PlexAccountServers).ThenInclude(x => x.PlexServer);

    public static IQueryable<PlexAccount> IncludeLibraryAccess(this IQueryable<PlexAccount> plexAccount) =>
        plexAccount.Include(x => x.PlexAccountLibraries).ThenInclude(x => x.PlexLibrary);
}
