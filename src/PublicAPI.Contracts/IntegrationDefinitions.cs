namespace Reaparr.PublicAPI.Contracts;

public static class IntegrationDefinitions
{
    // ReSharper disable once InconsistentNaming
    public const string RADARR_DEFAULT_CATEGORY = "reaparr-radarr";

    // ReSharper disable once InconsistentNaming
    public const string SONARR_DEFAULT_CATEGORY = "reaparr-sonarr";

    public const string IntegrationIdentityItemKey = "IntegrationIdentity";

    // ReSharper disable once InconsistentNaming
    public const string INDEXER_API_KEY = "apikey";

    public static IReadOnlyList<TorznabCategoryId> SupportedTorznabCategories { get; } =
    [
        // Movies
        TorznabCategoryId.Movies,
        TorznabCategoryId.Movies_Foreign,
        TorznabCategoryId.Movies_SD,
        TorznabCategoryId.Movies_HD,
        TorznabCategoryId.Movies_UHD,
        TorznabCategoryId.Movies_BluRay,
        TorznabCategoryId.Movies_WEBDL,

        // TV
        TorznabCategoryId.TV,
        TorznabCategoryId.TV_Foreign,
        TorznabCategoryId.TV_SD,
        TorznabCategoryId.TV_HD,
        TorznabCategoryId.TV_UHD,
        TorznabCategoryId.TV_Sport,
        TorznabCategoryId.TV_Anime,
        TorznabCategoryId.TV_Documentary,
    ];
}
