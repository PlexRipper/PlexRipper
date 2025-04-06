using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace PlexRipper.BaseTests;

public static class PlexApiMockDataExtensions
{
    public static void SetParentValues(this GetLibraryItemsMetadata destination, GetLibraryItemsMetadata source)
    {
        destination.ParentKey = source.Key;
        destination.ParentIndex = source.Index;
        destination.ParentGuid = source.Guid;
        destination.ParentRatingKey = source.RatingKey;
        destination.ParentSlug = source.Slug;
        destination.ParentStudio = source.Studio;
        destination.ParentTitle = source.Title;
        destination.ParentYear = source.Year;
        destination.ParentThumb = source.Thumb;
    }

    public static void SetGrandparentValues(this GetLibraryItemsMetadata destination, GetLibraryItemsMetadata source)
    {
        destination.GrandparentKey = source.Key;
        destination.GrandparentGuid = source.Guid;
        destination.GrandparentRatingKey = source.RatingKey;
        destination.GrandparentSlug = source.Slug;
        destination.GrandparentTitle = source.Title;
        destination.GrandparentThumb = source.Thumb;
    }
}
