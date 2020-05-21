using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlexRipper.Application.Services;
using PlexRipper.Infrastructure.Common.DTO;
using PlexRipper.Infrastructure.Repositories;
using System.IO;

namespace PlexRipper.Application.IntegrationTests.Base
{
    public class BaseServiceTest
    {
        public BaseServiceTest()
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
            LoggerExtensions.LogWarning(BaseDependanciesTest
                    .GetLogger<BaseServiceTest>(), "MAKE SURE TO CREATE A \"secretCredentials.json\" IN THE Infrastructure.UnitTests project TO START TESTING!");
            return new CredentialsDTO();
        }

        public static PlexService GetPlexService()
        {
            return new PlexService(
                BaseDependanciesTest.GetDbContext(),
                BaseDependanciesTest.GetMapper(),
                BaseApiTest.GetPlexApiService(),
                BaseDependanciesTest.GetLogger<PlexService>());
        }

        public static AccountService GetAccountService()
        {
            return new AccountService(
                BaseDependanciesTest.GetDbContext(),
                new AccountRepository(BaseDependanciesTest.GetDbContext()),
                BaseDependanciesTest.GetMapper(),
                GetPlexService(),
                BaseDependanciesTest.GetLogger<AccountService>());
        }
    }
}
