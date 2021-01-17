using RestSharp;

namespace PlexRipper.PlexApi.Helpers
{
    public static class PlexApiExtensions
    {

        /// <summary>
        /// Adds the required headers and also the authorization header.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="authToken"></param>
        public static RestRequest AddToken(this RestRequest request, string authToken)
        {
            request.AddQueryParameter("X-Plex-Token", authToken);
            return request;
        }

        public static RestRequest AddLimitHeaders(this RestRequest request, int from, int to)
        {
            request.AddQueryParameter("X-Plex-Container-Start", from.ToString());
            request.AddQueryParameter("X-Plex-Container-Size", to.ToString());
            return request;
        }


    }
}