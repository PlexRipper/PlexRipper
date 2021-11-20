using System.Collections.Generic;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager
{
    public static class DownloadTaskActions
    {
        private const string StatusDetails = "details";

        private const string StatusDelete = "delete";

        private const string StatusStart = "start";

        private const string StatusPause = "pause";

        private const string StatusStop = "stop";

        private const string StatusClear = "clear";

        private const string StatusRestart = "restart";

        public static List<string> Convert(DownloadStatus downloadStatus)
        {
            var actions = new List<string>
            {
                StatusDetails,
            };

            switch (downloadStatus)
            {
                case DownloadStatus.Unknown:
                    actions.Add(StatusDelete);
                    break;
                case DownloadStatus.Initialized:
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
    }
}