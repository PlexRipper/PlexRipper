using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace Reaparr.BaseTests;

public static class PlexApiMockDataExtensions
{
    public static void SetParentValues(this GetMediaMetaDataMetadata destination, GetMediaMetaDataMetadata source)
    {
        destination.ParentKey = source.Key;
        destination.ParentIndex = source.Index;
        destination.ParentGuid = source.Guid;
        destination.ParentRatingKey = source.RatingKey;
        destination.ParentTitle = source.Title;
        destination.ParentThumb = source.Thumb;
    }

    public static void SetGrandparentValues(this GetMediaMetaDataMetadata destination, GetMediaMetaDataMetadata source)
    {
        destination.GrandparentKey = source.Key;
        destination.GrandparentGuid = source.Guid;
        destination.GrandparentRatingKey = source.RatingKey;
        destination.GrandparentSlug = source.Slug;
        destination.GrandparentTitle = source.Title;
        destination.GrandparentThumb = source.Thumb;
    }
}
