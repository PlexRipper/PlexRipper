using Newtonsoft.Json;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.ValueObjects;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Services
{
    public class PlexService : IPlexService
    {

        private const string PlexSignInUrl = "https://plex.tv/users/sign_in.json";
        private const string FriendsUri = "https://plex.tv/pms/friends/all";

        private const string GetAccountUri = "https://plex.tv/users/account.json";
        private const string PlexServersUrl = "https://plex.tv/pms/servers.xml";

        private static readonly HttpClient client = new HttpClient();

        public PlexService()
        {
            client.Timeout = new TimeSpan(0, 0, 0, 30);
        }

        public async Task<PlexAccount> RequestTokenAsync(string username, string password)
        {
            // Convert to Base64 encoded string
            string base64AuthInfo = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            // Construct request headers
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(PlexSignInUrl),
                Method = HttpMethod.Post,
            };

            request.Headers.Add("X-Plex-Version", "1.1.0");
            request.Headers.Add("X-Plex-Product", "Saverr");
            request.Headers.Add("X-Plex-Client-Identifier", "271938");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64AuthInfo);

            // Send request to Plex servers
            PlexAccount plexAccount = null;
            var task = await client.SendAsync(request).ContinueWith(async response =>
            {
                if (response.IsCompletedSuccessfully)
                {
                    var responseBody = await response.Result.Content.ReadAsStringAsync(); ;
                    plexAccount = JsonConvert.DeserializeObject<PlexAccount>(responseBody);
                }

                // TODO Add Error logging here
            });

            return plexAccount;
        }
    }
}
