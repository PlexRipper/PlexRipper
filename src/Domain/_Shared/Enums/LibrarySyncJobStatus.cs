namespace Reaparr.Domain;

/// <summary>
/// Represents the status of a library sync queue item.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LibrarySyncJobStatus
{
    /// <summary>
    /// String value was unable to be parsed to this enum.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Unknown))]
    Unknown = 0,

    /// <summary>
    /// Library sync is queued and waiting to be processed.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Queued))]
    Queued = 1,

    /// <summary>
    /// Library sync is currently being processed.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Processing))]
    Processing = 2,

    /// <summary>
    /// Library sync completed successfully.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Completed))]
    Completed = 3,

    /// <summary>
    /// Library sync failed with an error.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Failed))]
    Failed = 4,

    /// <summary>
    /// Library sync was cancelled.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Cancelled))]
    Cancelled = 5,
}
