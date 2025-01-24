using System.Text.Json.Serialization;

namespace PlexRipper.Domain;

/// <summary>
/// The key to identify various types of <see cref="DownloadTaskGeneric">DownloadTasks</see>.
/// </summary>
public record DownloadTaskKey
{
    [JsonPropertyName("type")]
    public required DownloadTaskType Type { get; init; }

    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("plexServerId")]
    public required int PlexServerId { get; init; }

    [JsonPropertyName("plexLibraryId")]
    public required int PlexLibraryId { get; init; }

    public bool IsValid => Id != Guid.Empty && PlexServerId > 0 && PlexLibraryId > 0 && Type != DownloadTaskType.None;

    public bool IsDownloadable => Type.IsDownloadable();

    // TODO:add title when creating the key

    public virtual bool Equals(DownloadTaskKey? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Type == other.Type
            && Id.Equals(other.Id)
            && PlexServerId == other.PlexServerId
            && PlexLibraryId == other.PlexLibraryId;
    }

    public override int GetHashCode() => HashCode.Combine((int)Type, Id, PlexServerId, PlexLibraryId);

    public override string ToString() => $"{Type} Id: {Id}, Server: {PlexServerId}, Library: {PlexLibraryId}";
}
