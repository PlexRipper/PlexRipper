namespace PlexRipper.Domain;

public class DownloadTaskTvShowEpisodeFile : DownloadTaskFileBase
{
    #region Relationships

    public DownloadTaskTvShowEpisode Parent { get; set; }

    public Guid ParentId { get; set; }

    #endregion

    #region Helpers

    public override void SetNull()
    {
        base.SetNull();
        Parent = null;
    }

    public override PlexMediaType MediaType => PlexMediaType.Episode;

    public override DownloadTaskType DownloadTaskType => DownloadTaskType.EpisodeData;

    public override bool IsDownloadable => true;

    public override int Count => 1;

    #endregion
}