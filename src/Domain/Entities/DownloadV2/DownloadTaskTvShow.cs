namespace PlexRipper.Domain;

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
    public override int Count => Children.Sum(x => x.Count) + 1;

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
        DownloadStatus = DownloadTaskActions.Aggregate(Children.Select(x => x.DownloadStatus).ToList());
    }

    #endregion public override bool IsDownloadable => true;
}