using System;

namespace PlexRipper.Application.Common.Models
{
    public class DownloadRequest
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public string AuthToken { get; set; }


        public Uri DownloadUri => new Uri(DownloadUrl, UriKind.Absolute);

        public string DownloadUrl => $"{Url}?X-Plex-Token={AuthToken}";

        public DownloadRequest(string downloadUrl, string serverAuthToken, string fileName)
        {
            Url = downloadUrl;
            AuthToken = serverAuthToken;
            FileName = fileName;
        }
    }
}
