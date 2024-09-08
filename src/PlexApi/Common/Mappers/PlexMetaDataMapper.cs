using PlexRipper.PlexApi.Models;

namespace PlexRipper.PlexApi;

public static partial class PlexMetaDataMapper
{
    #region Single Conversions

    public static PlexMovie ToPlexMovie(this Metadata source)
    {
        var plexMovie = source.ToPlexMedia().ToPlexMovie();

        plexMovie.FullTitle = $"{source.Title} ({source.Year})";
        return plexMovie;
    }

    public static PlexTvShow ToPlexTvShow(this Metadata source)
    {
        var plexTvShow = source.ToPlexMedia().ToPlexTvShow();
        plexTvShow.FullTitle = source.Title;
        return plexTvShow;
    }

    public static PlexTvShowSeason ToPlexTvShowSeason(this Metadata source)
    {
        var plexTvShowSeason = source.ToPlexMedia().ToPlexTvShowSeason();
        plexTvShowSeason.FullTitle = $"{source.ParentTitle}/{source.Title}";
        plexTvShowSeason.ParentKey = source.ParentRatingKey != null ? int.Parse(source.ParentRatingKey) : -1;
        return plexTvShowSeason;
    }

    public static PlexTvShowEpisode ToPlexTvShowEpisode(this Metadata source)
    {
        var plexTvShowSeason = source.ToPlexMedia().ToPlexTvShowEpisode();
        plexTvShowSeason.FullTitle = $"{source.GrandparentTitle}/{source.ParentTitle}/{source.Title}";
        plexTvShowSeason.ParentKey = source.ParentRatingKey != null ? int.Parse(source.ParentRatingKey) : -1;
        return plexTvShowSeason;
    }

    public static PlexMedia ToPlexMedia(this Metadata source)
    {
        return new PlexMedia
        {
            Id = 0,
            Title = source.Title,
            Year = source.Year,
            SortTitle = source.Title.ToSortTitle(),
            SearchTitle = source.Title.ToSearchTitle(),
            Guid = source.Guid,

            // Duration is in milliseconds and we want seconds
            Duration = source.Duration / 1000,
            MediaSize = source.Media.Sum(y => y.Part.Sum(z => z.Size)),
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            MediaData = new PlexMediaContainer() { MediaData = source.Media.ToPlexMediaData() },

            Type = PlexMediaType.None,
            Key = source.RatingKey != null ? int.Parse(source.RatingKey) : -1,
            MetaDataKey = RetrieveMetaDataKey(source),
            Studio = source.Studio ?? string.Empty,
            Summary = source.Summary ?? string.Empty,
            ContentRating = source.ContentRating ?? string.Empty,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt.ToDateTime(),
            Index = source.Index,
            HasThumb = !string.IsNullOrEmpty(source.Thumb),
            HasArt = !string.IsNullOrEmpty(source.Art),
            HasBanner = !string.IsNullOrEmpty(source.Banner),
            HasTheme = !string.IsNullOrEmpty(source.Theme),

            // Ignore the following
            FullTitle = string.Empty,
            PlexLibrary = default,
            PlexServer = default,
            PlexLibraryId = default,
            PlexServerId = default,
            FullThumbUrl = string.Empty,
            FullBannerUrl = string.Empty,
        };
    }

    #endregion

    #region List Conversions

    public static List<PlexMovie> ToPlexMovies(this List<Metadata> source) => source.ConvertAll(ToPlexMovie);

    public static List<PlexTvShow> ToPlexTvShows(this List<Metadata> source) => source.ConvertAll(ToPlexTvShow);

    public static List<PlexTvShowSeason> ToPlexTvShowSeasons(this List<Metadata> source) =>
        source.ConvertAll(ToPlexTvShowSeason);

    public static List<PlexTvShowEpisode> ToPlexTvShowEpisodes(this List<Metadata> source) =>
        source.ConvertAll(ToPlexTvShowEpisode);

    #endregion

    /// <summary>
    /// Retrieves the MetaDataKey from either the ThumbUrl,BannerUrl, ArtUrl or ThemeUrl.
    /// It is assumed that all MetaDataKeys are the same, returns 0 if nothing is found.
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    private static int RetrieveMetaDataKey(Metadata metadata)
    {
        List<string> list = [metadata.Thumb, metadata.Banner, metadata.Art, metadata.Theme];

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
