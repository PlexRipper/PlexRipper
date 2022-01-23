using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public class PlexMediaData
    {
        public string MediaFormat { get; set; }

        public long Duration { get; set; }

        public string VideoResolution { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int Bitrate { get; set; }

        public string VideoCodec { get; set; }

        public string VideoFrameRate { get; set; }

        public double AspectRatio { get; set; }

        public string VideoProfile { get; set; }

        public string AudioProfile { get; set; }

        public string AudioCodec { get; set; }

        public int AudioChannels { get; set; }

        public bool OptimizedForStreaming { get; set; }

        public string Protocol { get; set; }

        public bool Selected { get; set; }

        public List<PlexMediaDataPart> Parts { get; set; }

        public bool IsMultiPart => Parts.Count > 1;
    }
}