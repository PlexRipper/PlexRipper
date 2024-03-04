namespace PlexRipper.Domain;

public class DownloadTaskMovieFile : DownloadTaskFileBase
{
    #region Relationships

    public DownloadTaskMovie Parent { get; set; }

    public Guid ParentId { get; set; }

    #endregion

    #region Helpers

    public override void SetNull()
    {
        base.SetNull();
        Parent = null;
    }

    public override PlexMediaType MediaType => PlexMediaType.Movie;

    public override DownloadTaskType DownloadTaskType => DownloadTaskType.MovieData;

    public override bool IsDownloadable => true;

    #endregion
}