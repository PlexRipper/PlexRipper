namespace Reaparr.Data.Contracts;

public static partial class DownloadTaskExtensions
{
    public static IQueryable<T> WhereIntegrationIs<T>(this IQueryable<T> query, IntegrationIdentity? integration)
        where T : DownloadTaskBase =>
        integration?.Type switch
        {
            IntegrationType.Sonarr => query.Where(x =>
                x.SonarrIntegrationId == integration.Id && x.RadarrIntegrationId == null
            ),
            IntegrationType.Radarr => query.Where(x =>
                x.SonarrIntegrationId == null && x.RadarrIntegrationId == integration.Id
            ),
            _ => query.Where(x => x.SonarrIntegrationId == null && x.RadarrIntegrationId == null),
        };

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
