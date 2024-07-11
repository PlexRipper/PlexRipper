namespace PlexRipper.Domain
{
    public record PlexMediaData
    {
        public required string MediaFormat { get; set; }
        public required long Duration { get; set; }
        public required string VideoResolution { get; set; }
        public required int Width { get; set; }
        public required int Height { get; set; }
        public required int Bitrate { get; set; }
        public required string VideoCodec { get; set; }
        public required string VideoFrameRate { get; set; }
        public required double AspectRatio { get; set; }
        public required string VideoProfile { get; set; }
        public required string AudioProfile { get; set; }
        public required string AudioCodec { get; set; }
        public required int AudioChannels { get; set; }
        public required bool OptimizedForStreaming { get; set; }
        public required string Protocol { get; set; }
        public required bool Selected { get; set; }
        public required List<PlexMediaDataPart> Parts { get; set; }

        public bool IsMultiPart => Parts.Count > 1;
    }
}
