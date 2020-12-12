using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PlexRipper.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DownloadStatus
    {
        // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
        // Otherwise the Typescript DTO translator in the front-end starts messing up

        /// <summary>
        /// String value was unable to be parsed to this enum.
        /// </summary>
        [EnumMember(Value = "Unknown")]
        Unknown = 0,

        /// <summary>
        /// Download Task is created and set to default values.
        /// </summary>
        [EnumMember(Value = "Initialized")]
        Initialized = 1,

        /// <summary>
        /// Download Task is created and is preparing to start downloading.
        /// </summary>
        [EnumMember(Value = "Starting")]
        Starting = 2,

        /// <summary>
        /// Download Task is downloading data from the server.
        /// </summary>
        [EnumMember(Value = "Downloading")]
        Downloading = 3,

        /// <summary>
        /// Download Task releases used resources, so the download can be paused.
        /// </summary>
        [EnumMember(Value = "Pausing")]
        Pausing = 4,

        /// <summary>
        /// Download is paused.
        /// </summary>
        [EnumMember(Value = "Paused")]
        Paused = 5,

        /// <summary>
        /// Download Task releases used resources, so the download can be paused.
        /// </summary>
        [EnumMember(Value = "Stopping")]
        Stopping = 6,

        /// <summary>
        /// Download is paused.
        /// </summary>
        [EnumMember(Value = "Stopped")]
        Stopped = 7,

        /// <summary>
        /// Download is added to the queue.
        /// </summary>
        [EnumMember(Value = "Queued")]
        Queued = 8,

        /// <summary>
        /// Download Task releases used resources, so the download can be deleted.
        /// </summary>
        [EnumMember(Value = "Deleting")]
        Deleting = 9,

        /// <summary>
        /// Download is deleted.
        /// </summary>
        [EnumMember(Value = "Deleted")]
        Deleted = 10,

        /// <summary>
        /// Download segments are being merged into 1 media file.
        /// </summary>
        [EnumMember(Value = "Merging")]
        Merging = 11,

        /// <summary>
        /// Download is completed.
        /// </summary>
        [EnumMember(Value = "Completed")]
        Completed = 12,

        /// <summary>
        /// There was an error during download.
        /// </summary>
        [EnumMember(Value = "Error")]
        Error = 13,
    }
}