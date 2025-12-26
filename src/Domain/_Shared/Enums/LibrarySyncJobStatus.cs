using System.Runtime.Serialization;
using System.Text.Json.Serialization;

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
    [EnumMember(Value = nameof(Unknown))]
    Unknown = 0,

    /// <summary>
    /// Library sync is queued and waiting to be processed.
    /// </summary>
    [EnumMember(Value = nameof(Queued))]
    Queued = 1,

    /// <summary>
    /// Library sync is currently being processed.
    /// </summary>
    [EnumMember(Value = nameof(Processing))]
    Processing = 2,

    /// <summary>
    /// Library sync completed successfully.
    /// </summary>
    [EnumMember(Value = nameof(Completed))]
    Completed = 3,

    /// <summary>
    /// Library sync failed with an error.
    /// </summary>
    [EnumMember(Value = nameof(Failed))]
    Failed = 4,
}
