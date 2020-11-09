using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace PlexRipper.Domain
{
    public class PlexMediaMetaData : BaseEntity
    {
        public string Title { get; set; }

        public string TitleTvShow { get; set; }

        public string TitleTvShowSeason { get; set; }

        public int RatingKey { get; set; }

        public long Duration { get; set; }

        public int Bitrate { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public double AspectRatio { get; set; }

        public int AudioChannels { get; set; }

        public string AudioCodec { get; set; }

        public string VideoCodec { get; set; }

        public string VideoResolution { get; set; }

        public string MediaFormat { get; set; }

        public string VideoFrameRate { get; set; }

        public string AudioProfile { get; set; }

        public string VideoProfile { get; set; }

        public string FilePath { get; set; }

        public string ObfuscatedFilePath { get; set; }

        #region Helpers

        [NotMapped]
        public string FileName => Path.GetFileName(FilePath);

        #endregion
    }
}