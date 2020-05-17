using Microsoft.Extensions.Logging;
using NUnit.Framework;
using PlexRipper.Domain.Entities;
using System.Threading.Tasks;

namespace Infrastructure.UnitTests.Services
{
    public class PlexServiceTests
    {

        [SetUp]
        public void Setup()
        {
            BaseDependanciesTest.Setup();
        }

        [Test]
        public async Task ShouldReturnValidApiToken()
        {
            // Arrange
            var plexService = BaseServiceTest.GetPlexService();
            var accountService = BaseServiceTest.GetAccountService();
            var credentials = BaseServiceTest.GetCredentials();

            //Act 
            var newAccount = new Account
            {
                Username = credentials.Username,
                Password = credentials.Password
            };

            var result = await accountService.ValidateAccountAsync(newAccount);
            var account = await accountService.AddOrUpdateAccountAsync(newAccount);

            string authToken = await plexService.GetPlexToken(account.PlexAccount);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(authToken);
        }


        [Test]
        public async Task ShouldReturnListOfServers()
        {
            // Arrange
            var plexService = BaseServiceTest.GetPlexService();
            var accountService = BaseServiceTest.GetAccountService();
            var credentials = BaseServiceTest.GetCredentials();
            //Act 
            var account = await accountService.AddOrUpdateAccountAsync(new Account
            {
                Username = credentials.Username,
                Password = credentials.Password
            });
            var result = await accountService.ValidateAccountAsync(account);
            var serverList = await plexService.GetServers(account.PlexAccount, true);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(serverList);
        }


        [Test]
        public async Task ShouldReturnPlexLibrary()
        {

            var plexService = BaseServiceTest.GetPlexService();
            var accountService = BaseServiceTest.GetAccountService();
            var credentials = BaseServiceTest.GetCredentials();

            await accountService.AddOrUpdateAccountAsync(new Account(credentials.Username, credentials.Password));
            var account = await accountService.GetAccountAsync(credentials.Username);

            var serverList = await plexService.GetServers(account.PlexAccount);
            var library = await plexService.GetLibrary(serverList[0]);

            Assert.IsNotNull(library);
        }




        [Test]
        public void ShouldLogDebugToUnitTestConsole()
        {
            var logger = BaseDependanciesTest.GetLogger<object>();
            logger.LogDebug("This is a debug string");
            logger.LogInformation("This is an information string");
            logger.LogError("This is a error string");
            logger.LogCritical("This is a critical string");
        }
    }
}
