using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Models
{
    /// <summary>
    /// Attributes:
    ///     TAG (str): 'Part'
    ///     server (:class:`~plexapi.server.PlexServer`): PlexServer object this is from.
    ///     initpath (str): Relative path requested when retrieving specified data.
    ///     media (:class:`~plexapi.media.Media`): Media object this part belongs to.
    ///     container (str): Container type of this media part (ex: avi).
    ///     duration (int): Length of this media part in milliseconds.
    ///     file (str): Path to this file on disk (ex: /media/Movies/Cars.(2006)/Cars.cd2.avi)
    ///     id (int): Unique ID of this media part.
    ///     indexes (str, None): None or SD.
    ///     key (str): Key used to access this media part (ex: /library/parts/46618/1389985872/file.avi).
    ///     size (int): Size of this file in bytes (ex: 733884416).
    ///     streams (list&lt;:class:`~plexapi.media.MediaPartStream`&gt;): List of streams in this media part.
    ///     exists (bool): Determine if file exists
    ///     accessible (bool): Determine if file is accessible
    /// </summary>
    public class Part
    {
        // General 
        [JsonPropertyName("id")]
        [JsonConverter(typeof(IntValueConverter))]
        public int Id { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("duration")]
        [JsonConverter(typeof(IntValueConverter))]
        public int Duration { get; set; }

        [JsonPropertyName("file")]
        public string File { get; set; }

        [JsonPropertyName("size")]
        [JsonConverter(typeof(LongValueConverter))]
        public long Size { get; set; }

        [JsonPropertyName("container")]
        public string Container { get; set; }
        
        [JsonPropertyName("videoProfile")]
        public string VideoProfile { get; set; }
        
        [JsonPropertyName("stream")]
        public Stream[] Stream { get; set; }

        // TV Show Episode
        [JsonPropertyName("audioProfile")]
        public string AudioProfile { get; set; }

        // Movie Section
        [JsonPropertyName("hasThumbnail")]
        public string HasThumbnail { get; set; }
        
        [JsonPropertyName("indexes")]
        public string Indexes { get; set; }
        
        [JsonPropertyName("hasChapterTextStream")]
        public bool? HasChapterTextStream { get; set; }
    }
}