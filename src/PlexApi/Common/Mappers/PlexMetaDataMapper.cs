namespace PlexRipper.PlexApi;

public static class PlexMetaDataMapper
{
    #region Single Conversions

    public static PlexMovie ToPlexMovie(this LibraryMediaItemDTO source) => source.ToPlexMedia().ToPlexMovie(source);

    public static PlexTvShow ToPlexTvShow(this LibraryMediaItemDTO source) => source.ToPlexMedia().ToPlexTvShow(source);

    public static PlexTvShowSeason ToPlexTvShowSeason(this LibraryMediaItemDTO source) =>
        source.ToPlexMedia().ToPlexTvShowSeason(source);

    public static PlexTvShowEpisode ToPlexTvShowEpisode(this LibraryMediaItemDTO source) =>
        source.ToPlexMedia().ToPlexTvShowEpisode(source);

    public static PlexMedia ToPlexMedia(this LibraryMediaItemDTO source)
    {
        return new PlexMedia
        {
            Id = 0,
            Title = source.Title,
            Year = source.Year,

            // This is set later on
            SortIndex = 0,

            SearchTitle = source.Title.ToSearchTitle(),
            Guid = source.Guid,

            Guid_IMDB = source.Guids.Find(x => x.Id.Contains("imdb"))?.Id.Replace("imdb://", "") ?? null,
            Guid_TMDB = source.Guids.Find(x => x.Id.Contains("tmdb"))?.Id.Replace("tmdb://", "") ?? null,
            Guid_TVDB = source.Guids.Find(x => x.Id.Contains("tvdb"))?.Id.Replace("tvdb://", "") ?? null,

            Duration = source.Duration,
            MediaSize = source.Media.Sum(y => y.Parts.Sum(z => z.Size)),
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            MediaData = source.Media,

            Type = PlexMediaType.None,
            Key = int.Parse(source.RatingKey),
            MetaDataKey = RetrieveMetaDataKey(source),
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt.ToDateTime(),
            HasThumb = !string.IsNullOrEmpty(source.Thumb),
            HasArt = !string.IsNullOrEmpty(source.Art),
            HasTheme = !string.IsNullOrEmpty(source.Theme),

            // Ignore the following
            FullTitle = string.Empty,
            PlexLibrary = default,
            PlexServer = default,
            PlexLibraryId = default,
            PlexServerId = default,
            FullBannerUrl = string.Empty,
        };
    }

    #endregion

    #region List Conversions

    public static List<PlexMovie> ToPlexMovies(this List<LibraryMediaItemDTO> source) =>
        source.Select(value => value.ToPlexMovie()).ToList();

    public static List<PlexTvShow> ToPlexTvShows(this List<LibraryMediaItemDTO> source) =>
        source.Select(value => value.ToPlexTvShow()).ToList();

    public static List<PlexTvShowSeason> ToPlexTvShowSeasons(this List<LibraryMediaItemDTO> source) =>
        source.Select(value => value.ToPlexTvShowSeason()).ToList();

    public static List<PlexTvShowEpisode> ToPlexTvShowEpisodes(this List<LibraryMediaItemDTO> source) =>
        source.Select(value => value.ToPlexTvShowEpisode()).ToList();

    #endregion

    /// <summary>
    /// Retrieves the MetaDataKey from either the ThumbUrl,BannerUrl, ArtUrl or ThemeUrl.
    /// It is assumed that all MetaDataKeys are the same, returns 0 if nothing is found.
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    private static int RetrieveMetaDataKey(LibraryMediaItemDTO metadata)
    {
        List<string> list = [metadata.Thumb, metadata.Art, metadata.Theme];

        foreach (var entry in list)
            if (!string.IsNullOrEmpty(entry))
            {
                // We want the last number
                // Example: /library/metadata/457047/thumb/1587006741
                var splitStrings = entry.Split('/').ToList();
                if (splitStrings.Count > 2)
                {
                    if (int.TryParse(splitStrings.Last(), out var result))
                        return result;
                }
            }

        return 0;
    }
}
