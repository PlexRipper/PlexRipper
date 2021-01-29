using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager.Common
{
    public class DownloadProgress : IDownloadProgress
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("percentage", Required = Required.Always)]
        public decimal Percentage => decimal.Round(WorkerProgresses.AsQueryable().Average(x => x.Percentage), 2);

        [JsonProperty("downloadSpeed", Required = Required.Always)]
        public int DownloadSpeed => WorkerProgresses.AsQueryable().Sum(x => x.DownloadSpeed);

        [JsonProperty("dataReceived", Required = Required.Always)]
        public long DataReceived => WorkerProgresses.AsQueryable().Sum(x => x.DataReceived);

        [JsonProperty("dataTotal", Required = Required.Always)]
        public long DataTotal { get; set; }

        [JsonProperty("downloadSpeedFormatted", Required = Required.Always)]
        public string DownloadSpeedFormatted => DataFormat.FormatSpeedString(DownloadSpeed);

        /// <summary>
        /// The download time remaining in seconds.
        /// </summary>
        [JsonProperty("timeRemaining", Required = Required.Always)]
        public long TimeRemaining => DataFormat.GetTimeRemaining(BytesRemaining, DownloadSpeed);

        [JsonProperty("bytesRemaining", Required = Required.Always)]
        public long BytesRemaining => DataTotal - DataReceived;

        [JsonProperty("workerProgresses", Required = Required.Always)]
        public List<IDownloadWorkerProgress> WorkerProgresses { get; }

        [JsonProperty("plexServerId", Required = Required.Always)]
        public int PlexServerId { get; set; }

        [JsonProperty("plexLibraryId", Required = Required.Always)]
        public int PlexLibraryId { get; set; }

        public DownloadProgress(List<IDownloadWorkerProgress> progresses)
        {
            WorkerProgresses = progresses;
        }
    }
}