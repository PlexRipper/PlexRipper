using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PlexRipper.DownloadManager.Common
{
    public class DownloadProgress
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("status", Required = Required.Always)]
        public string Status { get; set; }

        [JsonProperty("percentage", Required = Required.Always)]
        public decimal Percentage => WorkerProgresses.AsQueryable().Average(x => x.Percentage);

        [JsonProperty("downloadSpeed", Required = Required.Always)]
        public long DownloadSpeed { get; set; }

        [JsonProperty("dataReceived", Required = Required.Always)]
        public long DataReceived => WorkerProgresses.AsQueryable().Sum(x => x.DataReceived);

        [JsonProperty("dataTotal", Required = Required.Always)]
        public long DataTotal { get; set; }

        /// <summary>
        /// The download time remaining in seconds
        /// </summary>
        [JsonProperty("timeRemaining", Required = Required.Always)]
        public int TimeRemaining { get; set; }

        public List<DownloadWorkerProgress> WorkerProgresses { get; }

        public DownloadProgress(List<DownloadWorkerProgress> progresses)
        {
            WorkerProgresses = progresses;
        }
    }
}