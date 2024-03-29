namespace PlexRipper.Domain;

public class DownloadTaskMovieFile : DownloadTaskFileBase
{
    #region Relationships

    public required DownloadTaskMovie Parent { get; set; }

    public required Guid ParentId { get; set; }

    #endregion

    #region Helpers

    public override int Count => 1;

    public override PlexMediaType MediaType => PlexMediaType.Movie;

    public override DownloadTaskType DownloadTaskType => DownloadTaskType.MovieData;

    public override bool IsDownloadable => true;

    #endregion
}