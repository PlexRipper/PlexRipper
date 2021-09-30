using System.IO;
using FluentResultExtensions.lib;
using Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public static class Secrets
    {
        public static TestCredentialsDTO GetCredentials()
        {
            // Create a secretCredentials.json in the root of Infrastructure.UnitTests project and add Plex testing credentials.
            // {
            //    "username": "SomeUsername",
            //    "password": "Password123"
            // }
            using (StreamReader r = new StreamReader("secretCredentials.json"))
            {
                string json = r.ReadToEnd();
                JObject o = JObject.Parse(json);
                if (o.ContainsKey("credentials"))
                {
                    return JsonConvert.DeserializeObject<TestCredentialsDTO>(json);
                }
            }

            Log.Error("No credentials found to use for integration testing!");
            return new TestCredentialsDTO();
        }

        /// <summary>
        /// Requires a valid account without 2FA enabled which has access to multiple servers.
        /// Needs to include 1 own server with a library
        /// </summary>
        public static TestAccountDTO Account1 => GetCredentials().Credentials.Count > 0 ? GetCredentials().Credentials[0] : new TestAccountDTO();

        /// <summary>
        /// Requires a valid account with 2FA enabled which has access to at least 1 server.
        /// Preferably access to the server of Account1
        /// </summary>
        public static TestAccountDTO Account2 => GetCredentials().Credentials.Count > 1 ? GetCredentials().Credentials[1] : new TestAccountDTO();
    }
}