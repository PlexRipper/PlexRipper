namespace Reaparr.PublicAPI;

public static class TorznabSearchHelpers
{
    public static IReadOnlySet<string>? GetRequestedAttributes(bool includeAllAttributes, string[] attributes) =>
        includeAllAttributes ? null : attributes.ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static TorznabMediaSearchResponseDTO CreateResponse(
        string description,
        int offset,
        int total,
        List<TorznabItem> items
    ) =>
        new()
        {
            Channel = new TorznabChannel
            {
                Title = "Reaparr Indexer",
                Description = description,
                Language = "en-us",
                Category = "search",
                Items = items,
                Response = new TorznabResponseMetadata { Offset = offset, Total = total },
            },
        };
}
