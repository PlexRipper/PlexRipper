using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Domain.Common;

namespace PlexRipper.DownloadManager.Common
{
    public class DownloadProgress : IDownloadProgress
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("status", Required = Required.Always)]
        public string Status { get; set; }

        [JsonProperty("percentage", Required = Required.Always)]
        public decimal Percentage => WorkerProgresses.AsQueryable().Average(x => x.Percentage);

        [JsonProperty("downloadSpeed", Required = Required.Always)]
        public int DownloadSpeed => WorkerProgresses.AsQueryable().Sum(x => x.DownloadSpeed);

        [JsonProperty("dataReceived", Required = Required.Always)]
        public long DataReceived => WorkerProgresses.AsQueryable().Sum(x => x.DataReceived);

        [JsonProperty("dataTotal", Required = Required.Always)]
        public long DataTotal { get; set; }

        public string DownloadSpeedFormatted => DataFormat.FormatSpeedString(DownloadSpeed);

        /// <summary>
        /// The download time remaining in seconds
        /// </summary>
        [JsonProperty("timeRemaining", Required = Required.Always)]
        public long TimeRemaining => DataFormat.GetTimeRemaining(BytesRemaining, DownloadSpeed);

        public long BytesRemaining => DataTotal - DataReceived;


        public List<IDownloadWorkerProgress> WorkerProgresses { get; }

        public DownloadProgress(List<IDownloadWorkerProgress> progresses)
        {
            WorkerProgresses = progresses;
        }
    }
}