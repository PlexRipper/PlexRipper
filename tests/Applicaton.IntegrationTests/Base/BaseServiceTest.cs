using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlexRipper.Application.Services;
using PlexRipper.Infrastructure.Common.DTO;
using PlexRipper.Infrastructure.Repositories;
using PlexRipper.Infrastructure.Services;
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
            //LoggerExtensions.LogWarning(BaseDependanciesTest
            //        .GetLogger<BaseServiceTest>(), "MAKE SURE TO CREATE A \"secretCredentials.json\" IN THE Infrastructure.UnitTests project TO START TESTING!");
            return new CredentialsDTO();
        }



        public static PlexApiService GetPlexApiService()
        {
            return new PlexApiService(BaseApiTest.GetPlexApi(), BaseDependanciesTest.GetMapper());
        }


        public static PlexAuthenticationService GetPlexAuthenticationService()
        {
            return new PlexAuthenticationService(GetPlexApiService(), BaseDependanciesTest.GetLogger<PlexAuthenticationService>());
        }

        public static PlexService GetPlexService()
        {
            return new PlexService(
                BaseDependanciesTest.GetDbContext(),
                GetPlexAccountRepository(),
                GetPlexServerService(),
                GetPlexServerRepository(),
                GetPlexAuthenticationService(),
                BaseApiTest.GetPlexApiService(),
                BaseDependanciesTest.GetMapper(),
                BaseDependanciesTest.GetLogger<PlexService>());
        }

        private static PlexAccountRepository GetPlexAccountRepository()
        {
            return new PlexAccountRepository(BaseDependanciesTest.GetDbContext(), BaseDependanciesTest.GetLogger<PlexAccountRepository>());
        }

        private static PlexServerRepository GetPlexServerRepository()
        {
            return new PlexServerRepository(BaseDependanciesTest.GetDbContext(), BaseDependanciesTest.GetLogger<PlexServerRepository>());
        }

        public static PlexServerService GetPlexServerService()
        {
            return new PlexServerService(
                new PlexServerRepository(BaseDependanciesTest.GetDbContext(), BaseDependanciesTest.GetLogger<PlexServerRepository>()),
                BaseApiTest.GetPlexApiService(),
                GetPlexAuthenticationService(),
                BaseDependanciesTest.GetLogger<PlexService>());
        }

        public static AccountService GetAccountService()
        {
            return new AccountService(
                BaseDependanciesTest.GetDbContext(),
                new AccountRepository(BaseDependanciesTest.GetDbContext(), BaseDependanciesTest.GetLogger<AccountRepository>()),
                BaseDependanciesTest.GetMapper(),
                GetPlexService(),
                GetPlexServerService(),
                BaseDependanciesTest.GetLogger<AccountService>());
        }
    }
}
