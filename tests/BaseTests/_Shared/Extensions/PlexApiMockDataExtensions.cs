using LukeHagar.PlexAPI.SDK.Models.Components;

namespace Reaparr.BaseTests;

public static class PlexApiMockDataExtensions
{
    public static void SetParentValues(this Metadata destination, Metadata source)
    {
        destination.ParentKey = source.Key;
        destination.ParentIndex = source.Index;
        destination.ParentGuid = source.Guid;
        destination.ParentRatingKey = source.RatingKey;
        destination.ParentTitle = source.Title;
        destination.ParentThumb = source.Thumb;
    }

    public static void SetGrandparentValues(this Metadata destination, Metadata source)
    {
        destination.GrandparentKey = source.Key;
        destination.GrandparentGuid = source.Guid;
        destination.GrandparentRatingKey = source.RatingKey;
        destination.GrandparentTitle = source.Title;
        destination.GrandparentThumb = source.Thumb;
    }
}
