namespace PlexRipper.Domain;

public record DownloadTaskGeneric
{
    public Guid Id { get; init; }

    public int Key { get; init; }

    public PlexMediaType MediaType { get; init; }

    public DownloadTaskType DownloadTaskType { get; init; }

    public bool IsDownloadable { get; init; }

    #region Relationships

    public List<DownloadTaskGeneric> Children { get; init; }

    public PlexServer PlexServer { get; set; }

    public int PlexServerId { get; set; }

    public PlexLibrary PlexLibrary { get; set; }

    public int PlexLibraryId { get; set; }

    public FolderPath DestinationFolder { get; set; }

    public int DestinationFolderId { get; set; }

    public FolderPath DownloadFolder { get; set; }

    public int DownloadFolderId { get; set; }

    #endregion
}