using System;
using System.Net;

namespace PlexRipper.DownloadManager.Common
{
    /// <summary>
    /// Source: https://stackoverflow.com/a/17129686/8205497
    /// </summary>
    public class ExtendedWebClient : WebClient
    {
        /// <summary>
        /// Gets or sets the maximum number of concurrent connections (default is 2).
        /// </summary>
        public int ConnectionLimit { get; set; }

        /// <summary>
        /// Creates a new instance of ExtendedWebClient.
        /// </summary>
        public ExtendedWebClient()
        {
            ConnectionLimit = 100;
        }

        /// <summary>
        /// Creates the request for this client and sets connection defaults.
        /// </summary>
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address) as HttpWebRequest;

            if (request != null)
            {
                request.ServicePoint.ConnectionLimit = ConnectionLimit;
            }

            return request;
        }
    }
}