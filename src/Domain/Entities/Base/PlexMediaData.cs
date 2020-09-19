using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain
{
    public class PlexMediaData : BaseEntity
    {
        #region Properties

        public double AspectRatio { get; set; }

        public string AudioProfile { get; set; }

        public string VideoProfile { get; set; }

        public int AudioChannels { get; set; }

        public string AudioCodec { get; set; }

        public int Bitrate { get; set; }

        public string MediaFormat { get; set; }

        public long Duration { get; set; }

        public int Height { get; set; }

        public bool OptimizedForStreaming { get; set; }

        public string Protocol { get; set; }

        public string VideoCodec { get; set; }

        public string VideoFrameRate { get; set; }

        public string VideoResolution { get; set; }

        public int Width { get; set; }

        public bool Selected { get; set; }

        #region Relationships

        [NotMapped]
        public List<PlexMediaDataPart> Parts { get; set; }

        /// <summary>
        /// Up to 8 parts of individual media files are supported by Plex.
        /// Src: https://support.plex.tv/articles/naming-and-organizing-your-movie-media-files/
        /// </summary>
        // public PlexMediaDataPart Part1 { get; set; }
        //
        // public int Part1Id { get; set; }
        //
        // public PlexMediaDataPart Part2 { get; set; }
        //
        // public int? Part2Id { get; set; }
        //
        // public PlexMediaDataPart Part3 { get; set; }
        //
        // public int? Part3Id { get; set; }
        //
        // public PlexMediaDataPart Part4 { get; set; }
        //
        // public int? Part4Id { get; set; }
        //
        // public PlexMediaDataPart Part5 { get; set; }
        //
        // public int? Part5Id { get; set; }
        //
        // public PlexMediaDataPart Part6 { get; set; }
        //
        // public int? Part6Id { get; set; }
        //
        // public PlexMediaDataPart Part7 { get; set; }
        //
        // public int? Part7Id { get; set; }
        //
        // public PlexMediaDataPart Part8 { get; set; }
        //
        // public int? Part8Id { get; set; }

        #endregion

        #endregion
    }
}