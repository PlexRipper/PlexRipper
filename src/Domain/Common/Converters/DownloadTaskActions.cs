namespace PlexRipper.Domain;

public static class DownloadTaskActions
{
    private const string StatusDetails = "details";

    private const string StatusDelete = "delete";

    private const string StatusStart = "start";

    private const string StatusPause = "pause";

    private const string StatusStop = "stop";

    private const string StatusClear = "clear";

    private const string StatusRestart = "restart";

    private static readonly List<DownloadStatus> AllStatuses =
        new()
        {
            DownloadStatus.Queued,
            DownloadStatus.Downloading,
            DownloadStatus.DownloadFinished,
            DownloadStatus.Completed,
            DownloadStatus.Deleted,
            DownloadStatus.Unknown,
            DownloadStatus.Merging,
            DownloadStatus.Moving,
        };

    private static readonly List<DownloadStatus> AnyStatuses =
        new() { DownloadStatus.Error, DownloadStatus.Downloading, DownloadStatus.Paused, DownloadStatus.Stopped, };

    public static List<string> Convert(DownloadStatus downloadStatus)
    {
        var actions = new List<string> { StatusDetails, };

        switch (downloadStatus)
        {
            case DownloadStatus.Unknown:
                actions.Add(StatusDelete);
                break;
            case DownloadStatus.Queued:
                actions.Add(StatusStart);
                actions.Add(StatusDelete);
                break;
            case DownloadStatus.Downloading:
                actions.Add(StatusPause);
                actions.Add(StatusStop);
                break;
            case DownloadStatus.DownloadFinished:
                actions.Add(StatusDelete);
                break;
            case DownloadStatus.Paused:
                actions.Add(StatusStart);
                actions.Add(StatusStop);
                actions.Add(StatusDelete);
                break;
            case DownloadStatus.Completed:
                actions.Add(StatusClear);
                actions.Add(StatusRestart);
                break;
            case DownloadStatus.Stopped:
                actions.Add(StatusRestart);
                actions.Add(StatusDelete);
                break;
            case DownloadStatus.Merging:
                break;
            case DownloadStatus.Error:
                actions.Add(StatusRestart);
                actions.Add(StatusDelete);
                break;
        }

        return actions;
    }

    /// <summary>
    /// Determines the overall <see cref="DownloadStatus"/> based on the child list of <see cref="DownloadStatus"/>
    /// </summary>
    /// <param name="downloadStatusList">The DownloadStatus list to aggregate from.</param>
    /// <returns>The aggregated <see cref="DownloadStatus"/>.</returns>
    public static DownloadStatus Aggregate(List<DownloadStatus> downloadStatusList)
    {
        foreach (var status in AnyStatuses.Where(status => downloadStatusList.Any(x => x == status)))
        {
            return status;
        }

        foreach (var status in AllStatuses.Where(status => downloadStatusList.All(x => x == status)))
        {
            return status;
        }

        if (
            downloadStatusList.Any(x => x == DownloadStatus.DownloadFinished)
            && downloadStatusList.Any(x => x == DownloadStatus.Queued)
        )
            return DownloadStatus.Downloading;

        if (
            downloadStatusList.Any(x => x == DownloadStatus.DownloadFinished)
            && downloadStatusList.Any(x => x == DownloadStatus.Completed)
        )
            return DownloadStatus.DownloadFinished;

        if (
            downloadStatusList.Any(x => x == DownloadStatus.Queued)
            && downloadStatusList.Any(x => x == DownloadStatus.Queued)
        )
            return DownloadStatus.Queued;

        return DownloadStatus.Unknown;
    }
}
