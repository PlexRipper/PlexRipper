namespace Reaparr.PlexApi;

public static partial class PlexMediaDataMapper
{
    public static List<PlexPhotoMediaData> ToPlexPhotoMediaDataList(this List<LibraryMediaItemDTO> source) =>
        source.Select(value => value.ToPlexPhotoMediaData()).ToList();

    public static PlexPhotoMediaData ToPlexPhotoMediaData(this LibraryMediaItemDTO source)
    {
        if (source.Media.Count != 1 || source.Media[0].Parts.Count != 1)
            throw new InvalidOperationException(
                "Plex photo assets must contain exactly one Media item with exactly one Part"
            );

        var media = source.Media[0];
        var part = media.Parts[0];
        var sourcePath = string.IsNullOrWhiteSpace(part.File) ? null : part.File;

        return new PlexPhotoMediaData
        {
            Id = 0,
            PlexLibraryId = 0,
            PlexServerId = 0,
            PlexApiRatingKey = source.RatingKey,
            PlexApiMediaId = media.Id,
            PlexApiPartId = part.Id,
            AssetKind = source.Type is PlexMediaType.OtherVideos or PlexMediaType.Movie
                ? PhotoAssetKind.VideoClip
                : PhotoAssetKind.Image,
            Title = source.Title,
            SearchTitle = source.SearchTitle,
            Summary = source.Summary,
            PlexApiMetaDataKey = RetrieveMetaDataKey(source),
            HasThumb = !string.IsNullOrEmpty(source.Thumb),
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            OriginallyAvailableAt = source.OriginallyAvailableAt.ToDateTime(),
            SourcePath = sourcePath,
            OriginalFilename = part.File.GetFileName(),
            Key = part.Key,
            Size = part.Size,
            Container = part.Container,
            Width = media.Width > 0 ? media.Width : null,
            Height = media.Height > 0 ? media.Height : null,
            Duration = part.Duration >= 0 ? part.Duration : media.Duration > 0 ? media.Duration : null,
            VideoCodec = string.IsNullOrWhiteSpace(media.VideoCodec) ? null : media.VideoCodec,
            AudioCodec = string.IsNullOrWhiteSpace(media.AudioCodec) ? null : media.AudioCodec,
        };
    }
}
