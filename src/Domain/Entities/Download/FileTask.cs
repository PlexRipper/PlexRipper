using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

/// <summary>
/// Used for the merging and transferring of the completed downloaded media file.
/// </summary>
public class FileTask : BaseEntity
{
    #region Properties

    public required DateTime CreatedAt { get; init; }

    public required string DestinationDirectory { get; init; }

    public required string FilePathsCompressed { get; init; }

    public required string FileName { get; init; }

    public required long FileSize { get; init; }

    public required Guid DownloadTaskId { get; init; }

    public required DownloadTaskType DownloadTaskType { get; init; }

    #region Relationships

    public PlexServer? PlexServer { get; init; }

    public required int PlexServerId { get; init; }

    public PlexLibrary? PlexLibrary { get; init; }

    public required int PlexLibraryId { get; init; }

    #endregion

    #region Helpers

    public string DestinationFilePath => Path.Join(DestinationDirectory, FileName);

    /// <summary>
    /// Gets a list of file paths that need to be merged and/or moved.
    /// </summary>
    [NotMapped]
    public List<string> FilePaths => FilePathsCompressed.Split(';').ToList();

    [NotMapped]
    public DownloadTaskKey DownloadTaskKey =>
        new()
        {
            Type = DownloadTaskType,
            Id = DownloadTaskId,
            PlexServerId = PlexServerId,
            PlexLibraryId = PlexLibraryId,
        };

    #endregion

    #endregion
}
