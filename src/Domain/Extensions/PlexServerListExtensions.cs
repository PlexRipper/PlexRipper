using NaturalSort.Extension;

namespace PlexRipper.Domain;

public static class PlexServerListExtensions
{
    #region Methods

    #region Public

    public static List<PlexServer> SortByOwnedOrder(this List<PlexServer> plexServers)
    {
        return plexServers
            .OrderByDescending(x => x.Owned)
            .ThenBy(x => x.Name, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();
    }

    #endregion

    #endregion
}