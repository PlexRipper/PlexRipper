using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace PlexRipper.Domain
{
    public class PlexMediaMetaData : BaseEntity
    {
        public required string Title { get; set; }
        public required string TitleTvShow { get; set; }
        public required string TitleTvShowSeason { get; set; }
        public required int RatingKey { get; set; }
        public required long Duration { get; set; }
        public required int Bitrate { get; set; }
        public required int Width { get; set; }
        public required int Height { get; set; }
        public required double AspectRatio { get; set; }
        public required int AudioChannels { get; set; }
        public required string AudioCodec { get; set; }
        public required string VideoCodec { get; set; }
        public required string VideoResolution { get; set; }
        public required string MediaFormat { get; set; }
        public required string VideoFrameRate { get; set; }
        public required string AudioProfile { get; set; }
        public required string VideoProfile { get; set; }
        public required string FilePath { get; set; }
        public required string ObfuscatedFilePath { get; set; }

        #region Helpers

        [NotMapped]
        public string FileName => Path.GetFileName(FilePath);

        #endregion
    }
}
