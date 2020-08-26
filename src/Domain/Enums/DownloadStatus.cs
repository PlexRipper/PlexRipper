
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PlexRipper.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DownloadStatus
    {
        /// <summary>
        /// String value was unable to be parsed to this enum.
        /// </summary>
        [EnumMember(Value = "Unknown")]
        Unknown = 0,

        /// <summary>
        /// Download Task is created and set to default values
        /// </summary>
        [EnumMember(Value = "Initialized")]
        Initialized = 1,

        /// <summary>
        /// Download Task is created and is preparing to start downloading
        /// </summary>
        [EnumMember(Value = "Starting")]
        Starting = 2,

        /// <summary>
        /// Download Task is downloading data from the server
        /// </summary>
        [EnumMember(Value = "Downloading")]
        Downloading = 4,

        /// <summary>
        /// Download Task releases used resources, so the download can be paused
        /// </summary>
        [EnumMember(Value = "Pausing")]
        Pausing = 5,

        /// <summary>
        /// Download is paused
        /// </summary>
        [EnumMember(Value = "Paused")]
        Paused = 6,

        /// <summary>
        /// Download is added to the queue
        /// </summary>
        [EnumMember(Value = "Queued")]
        Queued = 7,

        /// <summary>
        /// Download Task releases used resources, so the download can be deleted
        /// </summary>
        [EnumMember(Value = "Deleting")]
        Deleting = 8,

        /// <summary>
        /// Download is deleted
        /// </summary>
        [EnumMember(Value = "Deleted")]
        Deleted = 9,

        /// <summary>
        /// Download is completed
        /// </summary>
        [EnumMember(Value = "Completed")]
        Completed = 10,

        /// <summary>
        /// There was an error during download
        /// </summary>
        [EnumMember(Value = "Error")]
        Error = 11,


    }
}
