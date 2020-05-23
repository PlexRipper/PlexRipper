using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlexRipper.Infrastructure.Common.DTO;
using System.IO;

namespace PlexRipper.Application.IntegrationTests.Base
{
    public class Secrets
    {
        public Secrets()
        {

        }

        public static CredentialsDTO GetCredentials()
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
                if (o.ContainsKey("username") && o.ContainsKey("password"))
                {
                    return JsonConvert.DeserializeObject<CredentialsDTO>(json);
                }

            }
            //LoggerExtensions.LogWarning(BaseDependanciesTest
            //        .GetLogger<BaseServiceTest>(), "MAKE SURE TO CREATE A \"secretCredentials.json\" IN THE Infrastructure.UnitTests project TO START TESTING!");
            return new CredentialsDTO();
        }

    }
}
