namespace Reaparr.PlexApi;

public static partial class PlexMediaDataMapper
{
    public static List<PlexMusicArtist> ToPlexMusicArtists(this List<LibraryMediaItemDTO> source) =>
        source.Select(value => value.ToPlexMusicArtist()).ToList();

    public static PlexMusicArtist ToPlexMusicArtist(this LibraryMediaItemDTO source) =>
        new()
        {
            Id = 0,
            Title = source.Title,
            Year = source.Year,
            SortIndex = source.SortIndex,
            SearchTitle = source.SearchTitle,
            Guid = source.Guid,
            Guid_IMDB = null,
            Guid_TMDB = null,
            Guid_TVDB = null,
            Duration = source.Duration,
            MediaSize = 0,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexApiRatingKey = source.RatingKey,
            PlexApiMetaDataKey = RetrieveMetaDataKey(source),
            Studio = string.Empty,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt.ToDateTime(),
            HasThumb = !string.IsNullOrEmpty(source.Thumb),
            HasArt = !string.IsNullOrEmpty(source.Art),
            HasTheme = !string.IsNullOrEmpty(source.Theme),
            Actors = source.Role.ToPlexActor(),
            Genres = source.Genre.ToPlexGenreAudio(),
            Countries = source.Country.ToPlexCountry(),
            FullTitle = source.Title,
            PlexLibraryId = 0,
            PlexServerId = 0,
            MusicBrainzArtistId = GetMusicBrainzId(source),
        };

}
