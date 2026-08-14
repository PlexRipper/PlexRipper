namespace Reaparr.Application;

internal static class PlexMediaComparisonDetailsMapper
{
    public static ComparisonDetailsRow ToComparisonDetailsRow(
        int rowId,
        int? parentRowId,
        int level,
        int plexMediaId,
        PlexMediaType type,
        string title,
        PlexMediaComparisonState state,
        VideoQuality remoteQuality,
        VideoQuality ownedQuality,
        string remoteLocation,
        string ownedLocation,
        int remotePlexLibraryId,
        int remotePlexServerId
    ) =>
        new()
        {
            RowId = rowId,
            ParentRowId = parentRowId,
            Level = level,
            PlexMediaId = plexMediaId,
            Type = type,
            Title = title,
            ComparisonId = state.ToComparisonId(),
            IsActionable = true,
            RemoteQuality = remoteQuality,
            OwnedQuality = ownedQuality,
            RemoteLocation = remoteLocation,
            OwnedLocation = ownedLocation,
            RemotePlexLibraryId = remotePlexLibraryId,
            RemotePlexServerId = remotePlexServerId,
        };

    public static List<PlexMediaComparisonDetailsRowDTO> ToDtoRows(List<ComparisonDetailsRow> rows)
    {
        var rowsByParentRowId = rows.GroupBy(x => x.ParentRowId ?? 0).ToDictionary(x => x.Key, x => x.ToList());
        return ToDtoRows(0, rowsByParentRowId);
    }

    public static PlexMediaComparisonState ToParentState(List<ComparisonDetailsRow> rows)
    {
        var hasMissing = rows.Any(x => x.ComparisonId == PlexMediaComparisonState.Missing.ToComparisonId());
        var hasPartial = rows.Any(x => x.ComparisonId == PlexMediaComparisonState.Partial.ToComparisonId());
        var hasHigherQuality = rows.Any(x => x.ComparisonId == PlexMediaComparisonState.HigherQuality.ToComparisonId());
        return (hasMissing, hasPartial, hasHigherQuality) switch
        {
            (true, false, false) => PlexMediaComparisonState.Missing,
            (_, true, true) => PlexMediaComparisonState.PartialAndHigherQuality,
            (_, true, false) => PlexMediaComparisonState.Partial,
            (true, false, true) => PlexMediaComparisonState.PartialAndHigherQuality,
            (false, false, true) => PlexMediaComparisonState.HigherQuality,
            _ => PlexMediaComparisonState.Owned,
        };
    }

    public static VideoQuality GetHighestQuality(IEnumerable<VideoQuality> qualities)
    {
        var values = qualities.ToList();
        return values.Count == 0 ? VideoQuality.None : values.Max();
    }

    private static List<PlexMediaComparisonDetailsRowDTO> ToDtoRows(
        int parentRowId,
        Dictionary<int, List<ComparisonDetailsRow>> rowsByParentRowId
    ) =>
        rowsByParentRowId
            .GetValueOrDefault(parentRowId, [])
            .Select(row => new PlexMediaComparisonDetailsRowDTO
            {
                PlexMediaId = row.PlexMediaId,
                Type = row.Type,
                Title = row.Title,
                State = row.ComparisonId.ToComparisonState(),
                RemoteQuality = row.RemoteQuality,
                OwnedQuality = row.OwnedQuality,
                PlexLibraryId = row.RemotePlexLibraryId,
                PlexServerId = row.RemotePlexServerId,
                Children = ToDtoRows(row.RowId, rowsByParentRowId),
            })
            .ToList();
}
