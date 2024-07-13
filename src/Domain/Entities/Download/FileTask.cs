using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

/// <summary>
/// Used for the merging and transferring of the completed downloaded media file.
/// </summary>
public class FileTask : BaseEntity
{
    #region Properties

    public required DateTime CreatedAt { get; set; }

    public required string DestinationDirectory { get; set; }

    public required string FilePathsCompressed { get; set; }

    public required string FileName { get; set; }

    public required long FileSize { get; set; }

    public required Guid DownloadTaskId { get; set; }

    public required DownloadTaskType DownloadTaskType { get; set; }

    #region Relationships

    public PlexServer? PlexServer { get; set; }

    public required int PlexServerId { get; set; }

    public PlexLibrary? PlexLibrary { get; set; }

    public required int PlexLibraryId { get; set; }

    #endregion

    [NotMapped]
    public DownloadTaskKey DownloadTaskKey =>
        new()
        {
            Type = DownloadTaskType,
            Id = DownloadTaskId,
            PlexServerId = PlexServerId,
            PlexLibraryId = PlexLibraryId,
        };

    #region Helpers

    public string DestinationFilePath => Path.Join(DestinationDirectory, FileName);

    /// <summary>
    /// Gets a list of file paths that need to be merged and/or moved.
    /// </summary>
    [NotMapped]
    public List<string> FilePaths => FilePathsCompressed?.Split(';').ToList() ?? new List<string>();

    #endregion

    #endregion
}
