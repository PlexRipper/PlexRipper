namespace PlexRipper.Domain;

public class DownloadTaskTvShowSeason : DownloadTaskParentBase
{
    #region Relationships

    public required List<DownloadTaskTvShowEpisode> Children { get; set; } = new();

    public required Guid ParentId { get; set; }

    public required DownloadTaskTvShow Parent { get; set; }

    #endregion

    #region Helpers

    public override void SetNull()
    {
        base.SetNull();
        Children = null;
    }

    public override PlexMediaType MediaType => PlexMediaType.Season;

    public override DownloadTaskType DownloadTaskType => DownloadTaskType.Season;

    public override bool IsDownloadable => false;

    public override int Count => Children.Sum(x => x.Count) + 1;

    #endregion
}