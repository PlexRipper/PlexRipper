using Logging.Interface;

namespace PlexRipper.Domain;

public static class DownloadTaskActions
{
    // ReSharper disable once InconsistentNaming
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(DownloadTaskActions));

    private const string StatusDetails = "details";

    private const string StatusDelete = "delete";

    private const string StatusStart = "start";

    private const string StatusPause = "pause";

    private const string StatusStop = "stop";

    private const string StatusClear = "clear";

    private const string StatusRestart = "restart";

    public static List<string> Convert(DownloadStatus downloadStatus)
    {
        var actions = new List<string> { StatusDetails };

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
            case DownloadStatus.MergeFinished:
            case DownloadStatus.MoveFinished:
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
            case DownloadStatus.Moving:
            case DownloadStatus.Merging:
                actions.Add(StatusPause);
                actions.Add(StatusStop);
                break;
            case DownloadStatus.MoveError:
            case DownloadStatus.MergeError:
                actions.Add(StatusDelete);
                break;
            case DownloadStatus.Error:
                actions.Add(StatusRestart);
                actions.Add(StatusDelete);
                break;
            case DownloadStatus.ServerUnreachable:
                actions.Add(StatusStart);
                actions.Add(StatusDelete);
                break;
            default:
                _log.Error("Unknown download status {DownloadStatus}", downloadStatus);
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
        // If any of these statuses are present, return that status.
        List<DownloadStatus> anyStatuses =
        [
            DownloadStatus.ServerUnreachable,
            DownloadStatus.Error,
            DownloadStatus.MoveError,
            DownloadStatus.MergeError,
            DownloadStatus.Downloading,
            DownloadStatus.Paused,
            DownloadStatus.Stopped,
            DownloadStatus.Merging,
            DownloadStatus.Moving,
        ];

        foreach (var status in anyStatuses.Where(status => downloadStatusList.Any(x => x == status)))
            return status;

        // Only return this status if all statuses are the same.
        List<DownloadStatus> allStatuses =
        [
            DownloadStatus.Queued,
            DownloadStatus.Downloading,
            DownloadStatus.DownloadFinished,
            DownloadStatus.Completed,
            DownloadStatus.Deleted,
            DownloadStatus.MergeFinished,
            DownloadStatus.MoveFinished,
            DownloadStatus.Unknown,
        ];
        foreach (var status in allStatuses.Where(status => downloadStatusList.All(x => x == status)))
            return status;

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

        _log.Error("Unable to determine the aggregate status of the download tasks. {StatusList}", downloadStatusList);

        return DownloadStatus.Unknown;
    }
}
