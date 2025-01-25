using Logging.Interface;

namespace PlexRipper.Domain;

public static class DownloadTaskActions
{
    // ReSharper disable once InconsistentNaming
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(DownloadTaskActions));

    public static List<DownloadActions> Convert(DownloadStatus downloadStatus)
    {
        var actions = new List<DownloadActions> { DownloadActions.Details };

        switch (downloadStatus)
        {
            case DownloadStatus.Unknown:
                actions.Add(DownloadActions.Delete);
                break;
            case DownloadStatus.Queued:
                actions.Add(DownloadActions.Start);
                actions.Add(DownloadActions.Delete);
                break;
            case DownloadStatus.Downloading:
                actions.Add(DownloadActions.Pause);
                actions.Add(DownloadActions.Stop);
                break;
            case DownloadStatus.DownloadFinished:
            case DownloadStatus.MergeFinished:
            case DownloadStatus.MoveFinished:
                actions.Add(DownloadActions.Delete);
                break;
            case DownloadStatus.Paused:
            case DownloadStatus.MergePaused:
            case DownloadStatus.MovePaused:
                actions.Add(DownloadActions.Start);
                actions.Add(DownloadActions.Stop);
                actions.Add(DownloadActions.Delete);
                break;
            case DownloadStatus.Completed:
                actions.Add(DownloadActions.Clear);
                actions.Add(DownloadActions.Restart);
                break;
            case DownloadStatus.Stopped:
                actions.Add(DownloadActions.Restart);
                actions.Add(DownloadActions.Delete);
                break;
            case DownloadStatus.Moving:
            case DownloadStatus.Merging:
                actions.Add(DownloadActions.Pause);
                actions.Add(DownloadActions.Stop);
                break;
            case DownloadStatus.Error:
            case DownloadStatus.MoveError:
            case DownloadStatus.MergeError:
                actions.Add(DownloadActions.Restart);
                actions.Add(DownloadActions.Delete);
                break;
            case DownloadStatus.ServerUnreachable:
                actions.Add(DownloadActions.Start);
                actions.Add(DownloadActions.Stop);
                actions.Add(DownloadActions.Delete);
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
        if (!downloadStatusList.Any())
        {
            _log.Warning(
                "{NameOfDownloadStatusList} was empty, cannot determine the aggregate status of the download tasks",
                nameof(downloadStatusList)
            );
            return DownloadStatus.Unknown;
        }

        // Only return this status if all statuses are the same.
        var allStatuses = Enum.GetValues<DownloadStatus>().ToList();
        foreach (var status in allStatuses.Where(status => downloadStatusList.All(x => x == status)))
            return status;

        // If any of these statuses are present, return that status.
        // Earlier statuses take precedence.
        List<DownloadStatus> anyStatuses =
        [
            DownloadStatus.ServerUnreachable,
            DownloadStatus.Error,
            DownloadStatus.MoveError,
            DownloadStatus.MergeError,
            DownloadStatus.Paused,
            DownloadStatus.MergePaused,
            DownloadStatus.MovePaused,
            DownloadStatus.Stopped,
            DownloadStatus.Downloading,
            DownloadStatus.Queued,
            DownloadStatus.Merging,
            DownloadStatus.Moving,
            DownloadStatus.MergeFinished,
            DownloadStatus.MoveFinished,
            DownloadStatus.DownloadFinished,
            DownloadStatus.Deleted,
            DownloadStatus.Completed,
        ];

        foreach (var status in anyStatuses.Where(status => downloadStatusList.Any(x => x == status)))
            return status;

        _log.Error("Unable to determine the aggregate status of the download tasks. {StatusList}", downloadStatusList);

        return DownloadStatus.Unknown;
    }
}
