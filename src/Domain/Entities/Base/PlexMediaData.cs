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

        #endregion

        #endregion
    }
}