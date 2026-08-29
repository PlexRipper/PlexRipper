namespace Reaparr.PublicAPI;

public static class PublicApiRoutes
{
    public const string Base = "/api/public/integrations/{integrationId:guid}";

    public const string IndexerBase = Base + "/indexer";

    public const string Indexer = IndexerBase + "/api";

    public const string DownloadTorrent = IndexerBase + "/download";

    public const string DownloadClientBase = Base + "/download-client";

    public const string DownloadClient = DownloadClientBase + "/api/v2";
}
