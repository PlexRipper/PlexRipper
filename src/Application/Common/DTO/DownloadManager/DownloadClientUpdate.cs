using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public class DownloadClientUpdate
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id => DownloadTask.Id;

        /// <summary>
        /// The formatted media title as shown in Plex.
        /// </summary>
        [JsonProperty("title", Required = Required.Always)]
        public string Title => DownloadTask.Title;

        /// <summary>
        /// The full media title including the [TvShow]/[Season]/[Episode] as shown in Plex.
        /// </summary>
        [JsonProperty("fullTitle", Required = Required.Always)]
        public string FullTitle => DownloadTask.TitlePath;

        /// <summary>
        /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv.
        /// </summary>
        [JsonProperty("fileLocationUrl", Required = Required.Always)]
        public string FileLocationUrl => DownloadTask.FileLocationUrl;

        [JsonProperty("fileName", Required = Required.Always)]
        public string FileName => DownloadTask.FileName;

        [JsonProperty("status", Required = Required.Always)]
        public DownloadStatus DownloadStatus => DownloadTask.DownloadStatus;

        [JsonProperty("percentage", Required = Required.Always)]
        public decimal Percentage => WorkerProgresses.Any() ? decimal.Round(WorkerProgresses.AsQueryable().Average(x => x.Percentage), 2) : 0;

        [JsonProperty("downloadSpeed", Required = Required.Always)]
        public int DownloadSpeed => WorkerProgresses.Any() ? WorkerProgresses.AsQueryable().Sum(x => x.DownloadSpeed) : 0;

        [JsonProperty("dataReceived", Required = Required.Always)]
        public long DataReceived => WorkerProgresses.AsQueryable().Sum(x => x.DataReceived);

        [JsonProperty("dataTotal", Required = Required.Always)]
        public long DataTotal => WorkerProgresses.AsQueryable().Sum(x => x.DataTotal);

        [JsonProperty("downloadSpeedFormatted", Required = Required.Always)]
        public string DownloadSpeedFormatted => DataFormat.FormatSpeedString(DownloadSpeed);

        /// <summary>
        /// Note: Naming third just 'type' will cause errors in the Typescript type generating.
        /// </summary>
        [JsonProperty("mediaType", Required = Required.Always)]
        public PlexMediaType MediaType => DownloadTask.MediaType;

        /// <summary>
        /// The download time remaining in seconds.
        /// </summary>
        [JsonProperty("timeRemaining", Required = Required.Always)]
        public long TimeRemaining => DataFormat.GetTimeRemaining(BytesRemaining, DownloadSpeed);

        [JsonProperty("bytesRemaining", Required = Required.Always)]
        public long BytesRemaining => DataTotal - DataReceived;

        [JsonProperty("workerProgresses", Required = Required.Always)]
        public List<DownloadWorkerUpdate> WorkerProgresses { get; }

        [JsonProperty("plexServerId", Required = Required.Always)]
        public int PlexServerId => DownloadTask.PlexServerId;

        [JsonProperty("plexLibraryId", Required = Required.Always)]
        public int PlexLibraryId => DownloadTask.PlexLibraryId;

        /// <summary>
        /// The identifier used by Plex to keep track of media.
        /// </summary>
        [JsonProperty("key", Required = Required.Always)]
        public int Key => DownloadTask.Key;

        [JsonProperty("downloadTask", Required = Required.Always)]
        public DownloadTask DownloadTask { get; }

        public DownloadClientUpdate(DownloadTask downloadTask, IList<DownloadWorkerUpdate> progresses = null)
        {
            DownloadTask = downloadTask;
            WorkerProgresses = progresses?.OrderBy(x => x.Id).ToList() ?? new List<DownloadWorkerUpdate>();
        }

        public override string ToString()
        {
            var orderedList = WorkerProgresses.ToList().OrderBy(x => x.Id).ToList();
            StringBuilder builder = new StringBuilder();
            foreach (var progress in orderedList)
            {
                builder.Append($"({progress.Id} - {progress.Percentage} {progress.DownloadSpeedFormatted}) + ");
            }

            // Remove the last " + "
            if (builder.Length > 3)
            {
                builder.Length -= 3;
            }

            builder.Append($" = ({DownloadSpeedFormatted} - {TimeRemaining})");

            return builder.ToString();
        }
    }
}