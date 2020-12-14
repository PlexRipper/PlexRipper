using System.Collections.Generic;
using PlexRipper.Domain;

namespace PlexRipper.PlexApi.Api
{
    public static class PlexHeaderData
    {
        public static Dictionary<string, string> GetBasicHeaders
        {
            get
            {
                var headers = new Dictionary<string, string>();

                // TODO Debate if we should put PlexRipper here
                headers.Add("User-Agent", "PlexClient");
                headers.Add("X-Plex-Product", "Plex Web");
                headers.Add("X-Plex-Version", "4.48.1");
                // TODO Will create a new string on every api request, might need to pick one and persist
                headers.Add("X-Plex-Client-Identifier", StringExtensions.RandomString(24, true));
                headers.Add("X-Plex-Platform", "Chrome");
                headers.Add("X-Plex-Platform-Version", "87.0");
                headers.Add("X-Plex-Sync-Version", "2");
                headers.Add("X-Plex-Features", "external-media,indirect-media");
                headers.Add("X-Plex-Model", "hosted");
                headers.Add("X-Plex-Device", "Windows");
                headers.Add("X-Plex-Device-Name", "Chrome");
                headers.Add("X-Plex-Device-Screen-Resolution", "440x1010,2048x1152");
                headers.Add("X-Plex-Language", "en");
                return headers;
            }
        }
    }
}