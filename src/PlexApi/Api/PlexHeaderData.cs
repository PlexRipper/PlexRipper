using System.Collections.Generic;

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
                headers.Add("X-Plex-Client-Identifier", "271938");
                headers.Add("X-Plex-Product", "Saverr");
                headers.Add("X-Plex-Version", "3");
                headers.Add("X-Plex-Device", "Ombi");
                headers.Add("X-Plex-Platform", "Web");
                return headers;
            }
        }
    }
}
