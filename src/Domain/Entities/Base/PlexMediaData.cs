using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain
{
    public class PlexMediaData : BaseEntity
    {
        #region Properties

        [Column(Order = 1)]
        public string MediaFormat { get; set; }

        [Column(Order = 2)]
        public long Duration { get; set; }

        [Column(Order = 3)]
        public string VideoResolution { get; set; }

        [Column(Order = 4)]
        public int Width { get; set; }

        [Column(Order = 5)]
        public int Height { get; set; }

        [Column(Order = 6)]
        public int Bitrate { get; set; }

        [Column(Order = 7)]
        public string VideoCodec { get; set; }

        [Column(Order = 8)]
        public string VideoFrameRate { get; set; }

        [Column(Order = 9)]
        public double AspectRatio { get; set; }

        [Column(Order = 10)]
        public string VideoProfile { get; set; }

        [Column(Order = 11)]
        public string AudioProfile { get; set; }

        [Column(Order = 12)]
        public string AudioCodec { get; set; }

        [Column(Order = 13)]
        public int AudioChannels { get; set; }

        public bool OptimizedForStreaming { get; set; }

        public string Protocol { get; set; }

        public bool Selected { get; set; }

        #endregion
    }
}