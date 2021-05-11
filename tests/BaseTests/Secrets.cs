using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            //LoggerExtensions.LogWarning(BaseDependanciesTest
            //        .GetLogger<BaseServiceTest>(), "MAKE SURE TO CREATE A \"secretCredentials.json\" IN THE Infrastructure.UnitTests project TO START TESTING!");
            return new TestCredentialsDTO();
        }

        public static TestAccountDTO Account1 => GetCredentials().Credentials.Count > 0 ? GetCredentials().Credentials[0] : new TestAccountDTO();

        public static TestAccountDTO Account2 => GetCredentials().Credentials.Count > 1 ? GetCredentials().Credentials[1] : new TestAccountDTO();
    }
}