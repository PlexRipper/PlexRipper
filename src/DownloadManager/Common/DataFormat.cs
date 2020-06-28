using System;
using System.Globalization;

namespace PlexRipper.DownloadManager.Common
{
    public static class DataFormat
    {
        private static readonly NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;

        /// <summary>
        /// Format file size or downloaded size string
        /// </summary>
        /// <param name="byteSize"></param>
        /// <returns></returns>
        public static string FormatSizeString(long byteSize)
        {
            double kiloByteSize = byteSize / 1024D;
            double megaByteSize = kiloByteSize / 1024D;
            double gigaByteSize = megaByteSize / 1024D;

            if (byteSize < 1024)
                return string.Format(numberFormat, "{0} B", byteSize);
            if (byteSize < 1048576)
                return string.Format(numberFormat, "{0:0.00} kB", kiloByteSize);
            if (byteSize < 1073741824)
                return string.Format(numberFormat, "{0:0.00} MB", megaByteSize);
            return string.Format(numberFormat, "{0:0.00} GB", gigaByteSize);
        }

        /// <summary>
        /// Format download speed string
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public static string FormatSpeedString(int speed)
        {
            float kbSpeed = speed / 1024F;
            float mbSpeed = kbSpeed / 1024F;

            if (speed <= 0)
                return string.Empty;
            if (speed < 1024)
                return speed + " B/s";
            if (speed < 1048576)
                return kbSpeed.ToString("#.00", numberFormat) + " kB/s";
            return mbSpeed.ToString("#.00", numberFormat) + " MB/s";
        }

        /// <summary>
        /// Format time span string so it can display values of more than 24 hours
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static string FormatTimeSpanString(TimeSpan span)
        {
            string hours = ((int)span.TotalHours).ToString();
            string minutes = span.Minutes.ToString();
            string seconds = span.Seconds.ToString();
            if ((int)span.TotalHours < 10)
                hours = "0" + hours;
            if (span.Minutes < 10)
                minutes = "0" + minutes;
            if (span.Seconds < 10)
                seconds = "0" + seconds;

            return $"{hours}:{minutes}:{seconds}";
        }

        public static float GetPercentage(long bytesReceived, long totalBytes)
        {
            return (float)Math.Round((bytesReceived / (double)totalBytes) * 100, 2, MidpointRounding.AwayFromZero);
        }
    }
}
