using Newtonsoft.Json;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Domain.Types
{
    public class LibraryProgress
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("percentage", Required = Required.Always)]
        public decimal Percentage { get; set; }

        [JsonProperty("received", Required = Required.Always)]
        public int Received { get; set; }

        [JsonProperty("total", Required = Required.Always)]
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="PlexLibrary"/> is currently refreshing the data from the external PlexServer
        /// or from our own local database.
        /// </summary>
        [JsonProperty("isRefreshing", Required = Required.Always)]
        public bool IsRefreshing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="PlexLibrary"/> has finished refreshing.
        /// </summary>
        [JsonProperty("isComplete", Required = Required.Always)]
        public bool IsComplete { get; set; }
    }
}