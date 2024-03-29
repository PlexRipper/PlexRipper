﻿using PlexRipper.Domain;

namespace Application.Contracts;

public class DownloadTaskDTO : IDownloadTaskProgress
{
    public Guid Id { get; set; }

    /// <summary>
    /// The identifier used by Plex to keep track of media.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The formatted media title as shown in Plex.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The full media title including the [TvShow]/[Season]/[Episode] as shown in Plex.
    /// </summary>
    public string FullTitle { get; set; }

    /// <summary>
    /// Note: Naming third just 'type' will cause errors in the Typescript type generating.
    /// </summary>
    public PlexMediaType MediaType { get; set; }

    public DownloadTaskType DownloadTaskType { get; set; }

    public DownloadStatus Status { get; set; }

    public decimal Percentage { get; set; }

    public long DataReceived { get; set; }

    public long DataTotal { get; set; }

    public DateTime CreatedAt { get; init; }

    public string FileName { get; set; }

    public long TimeRemaining { get; set; }
    public string DownloadDirectory { get; set; }

    public string DestinationDirectory { get; set; }

    /// <summary>
    /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv.
    /// </summary>
    public string FileLocationUrl { get; set; }

    public long DownloadSpeed { get; set; }

    public long FileTransferSpeed { get; set; }

    public string DownloadUrl { get; set; }

    #region Relationships

    public int PlexServerId { get; set; }

    public int PlexLibraryId { get; set; }

    public Guid ParentId { get; set; }

    #endregion

    public string Quality { get; set; }

    /// <summary>
    /// The nested <see cref="DownloadTaskGeneric"/> used for seasons and episodes.
    /// "Required = Required.Default" is used for ensuring it's optional in the Typescript generating.
    /// </summary>
    public List<DownloadTaskDTO> Children { get; set; }

    /// <summary>
    /// The actions that can be taken on this <see cref="DownloadTaskGeneric"/>.
    /// This is filled by the front-end and depends on the DownloadStatus
    /// </summary>
    public string[] Actions { get; set; }
}