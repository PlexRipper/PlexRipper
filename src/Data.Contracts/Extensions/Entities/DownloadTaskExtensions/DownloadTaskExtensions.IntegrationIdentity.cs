namespace Reaparr.Data.Contracts;

public static partial class DownloadTaskExtensions
{
    /// <summary>
    /// Filters tasks to one integration when supplied; a null identity intentionally leaves the query unfiltered.
    /// </summary>
    public static IQueryable<T> WhereIntegrationIs<T>(this IQueryable<T> query, IntegrationIdentity? integration)
        where T : DownloadTaskBase =>
        integration is null
            ? query
            : integration.Type switch
            {
                IntegrationType.Sonarr => query.Where(x =>
                    x.SonarrIntegrationId == integration.Id && x.RadarrIntegrationId == null
                ),
                IntegrationType.Radarr => query.Where(x =>
                    x.SonarrIntegrationId == null && x.RadarrIntegrationId == integration.Id
                ),
                _ => query.Where(x => x.SonarrIntegrationId == null && x.RadarrIntegrationId == null),
            };

    /// <summary>
    /// Filters tasks to the exact ownership represented by the identity; null means unowned tasks only.
    /// </summary>
    public static IQueryable<T> WhereIntegrationOwnershipMatches<T>(
        this IQueryable<T> query,
        IntegrationIdentity? integration
    )
        where T : DownloadTaskBase =>
        integration is null
            ? query.Where(x => x.SonarrIntegrationId == null && x.RadarrIntegrationId == null)
            : query.WhereIntegrationIs(integration);

    public static IQueryable<T> WhereIntegrationIsOrUnowned<T>(
        this IQueryable<T> query,
        IntegrationIdentity integration
    )
        where T : DownloadTaskBase =>
        integration.Type switch
        {
            IntegrationType.Sonarr => query.Where(x =>
                (x.SonarrIntegrationId == integration.Id && x.RadarrIntegrationId == null)
                || (x.SonarrIntegrationId == null && x.RadarrIntegrationId == null)
            ),
            IntegrationType.Radarr => query.Where(x =>
                (x.SonarrIntegrationId == null && x.RadarrIntegrationId == integration.Id)
                || (x.SonarrIntegrationId == null && x.RadarrIntegrationId == null)
            ),
            _ => query.Where(x => x.SonarrIntegrationId == null && x.RadarrIntegrationId == null),
        };
}
