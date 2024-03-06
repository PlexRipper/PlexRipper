namespace PlexRipper.Domain;

public class DownloadTaskTvShowEpisode : DownloadTaskParentBase
{
    #region Relationships

    public List<DownloadTaskTvShowEpisodeFile> Children { get; set; } = new();

    public Guid ParentId { get; set; }

    public DownloadTaskTvShowSeason Parent { get; set; }

    #endregion

    #region Helpers

    public override void SetNull()
    {
        base.SetNull();
        Children = null;
    }

    public override PlexMediaType MediaType => PlexMediaType.Episode;

    public override DownloadTaskType DownloadTaskType => DownloadTaskType.Episode;

    public override bool IsDownloadable => false;

    public override void Calculate()
    {
        if (!Children.Any())
            return;

        DataReceived = Children.Select(x => x.DataReceived).Sum();
        DataTotal = Children.Select(x => x.DataTotal).Sum();
        Percentage = DataFormat.GetPercentage(DataReceived, DataTotal);
        DownloadStatus = DownloadTaskActions.Aggregate(Children.Select(x => x.DownloadStatus).ToList());
    }

    #endregion
}