namespace Reaparr.PlexApi;

public static partial class PlexMediaDataMapper
{
    public static List<PlexPhotoAlbum> ToPlexPhotoAlbums(this List<LibraryMediaItemDTO> source) =>
        source.Select(value => value.ToPlexPhotoAlbum()).ToList();

    public static PlexPhotoAlbum ToPlexPhotoAlbum(this LibraryMediaItemDTO source) =>
        new()
        {
            Id = 0,
            PlexLibraryId = 0,
            PlexServerId = 0,
            PlexApiRatingKey = source.RatingKey,
            Title = source.Title,
            SearchTitle = source.SearchTitle,
            Summary = source.Summary,
            PlexApiMetaDataKey = RetrieveMetaDataKey(source),
            HasThumb = !string.IsNullOrEmpty(source.Thumb),
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
        };
}
