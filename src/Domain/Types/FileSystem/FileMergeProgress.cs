using Newtonsoft.Json;

namespace PlexRipper.Domain
{
    public class FileMergeProgress
    {
        /// <summary>
        /// This is equal to the <see cref="FileTask"/> Id.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// This is equal to the <see cref="DownloadTask"/> Id the <see cref="FileTask"/> is currently handling.
        /// </summary>
        [JsonProperty("downloadTaskId", Required = Required.Always)]
        public int DownloadTaskId { get; set; }

        [JsonProperty("dataTransferred", Required = Required.Always)]
        public long DataTransferred { get; set; }

        [JsonProperty("dataTotal", Required = Required.Always)]
        public long DataTotal { get; set; }

        [JsonProperty("percentage", Required = Required.Always)]
        public decimal Percentage => DataFormat.GetPercentage(DataTransferred, DataTotal);

        /// <summary>
        /// The transfer speed in bytes per second.
        /// </summary>
        [JsonProperty("transferSpeed", Required = Required.Always)]
        public int TransferSpeed { get; set; }

        [JsonProperty("transferSpeedFormatted", Required = Required.Always)]
        public string TransferSpeedFormatted => DataFormat.FormatSpeedString(TransferSpeed);

        /// <summary>
        /// The time remaining in seconds the <see cref="FileTask"/> to finish.
        /// </summary>
        [JsonProperty("timeRemaining", Required = Required.Always)]
        public long TimeRemaining => DataFormat.GetTimeRemaining(BytesRemaining, TransferSpeed);

        [JsonProperty("bytesRemaining", Required = Required.Always)]
        public long BytesRemaining => DataTotal - DataTransferred;
    }
}