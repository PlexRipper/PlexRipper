namespace PlexRipper.Domain.DownloadV2;

public class DownloadTaskTvShow : DownloadTaskParentBase
{
    #region Relationships

    public List<DownloadTaskTvShowSeason> Children { get; set; } = new();

    #endregion

    #region Helpers

    public override void SetNull()
    {
        base.SetNull();
        Children = null;
    }

    public override PlexMediaType MediaType => PlexMediaType.TvShow;

    public override DownloadTaskType DownloadTaskType => DownloadTaskType.TvShow;

    public override bool IsDownloadable => false;

    public override void Calculate()
    {
        if (!Children.Any())
            return;

        // Calculate children first
        foreach (var downloadTask in Children)
            downloadTask.Calculate();

        DataReceived = Children.Select(x => x.DataReceived).Sum();
        DataTotal = Children.Select(x => x.DataTotal).Sum();
        Percentage = DataFormat.GetPercentage(DataReceived, DataTotal);
    }

    #endregion public override bool IsDownloadable => true;
}