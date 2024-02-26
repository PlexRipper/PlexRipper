namespace PlexRipper.Domain.DownloadV2;

public class DownloadTaskMovieFile : DownloadTaskFileBase
{
    #region Relationships

    public DownloadTaskMovie Parent { get; set; }

    public int ParentId { get; set; }

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