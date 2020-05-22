using Autofac.Extras.Moq;
using PlexRipper.Application.IntegrationTests.Base;
using PlexRipper.Application.Services;
using PlexRipper.Domain.Entities;
using Shouldly;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests.Services
{
    public class PlexServiceIntegrationTests
    {

        public PlexServiceIntegrationTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.Setup(output);
        }


        [Fact]
        public async Task GetPlexToken_ShouldReturnValidApiToken()
        {
            using (var mock = AutoMock.GetLoose())
            {

                // Arrange
                var plexService = mock.Create<PlexService>();
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
                result.ShouldNotBeNull();
                authToken.ShouldNotBeEmpty();
            }

        }


        [Fact]
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
            var serverList = await plexService.GetServersAsync(account.PlexAccount, true);

            //Assert
            result.ShouldNotBeNull();
            serverList.ShouldNotBeEmpty();

        }


        [Fact]
        public async Task ShouldReturnPlexLibrary()
        {

            var plexService = BaseServiceTest.GetPlexService();
            var accountService = BaseServiceTest.GetAccountService();
            var credentials = BaseServiceTest.GetCredentials();

            await accountService.AddOrUpdateAccountAsync(new Account(credentials.Username, credentials.Password));
            var account = await accountService.GetAccountAsync(credentials.Username);

            var serverList = await plexService.GetServersAsync(account.PlexAccount);
            var library = await plexService.GetLibrary(serverList[0]);

            library.ShouldNotBeNull();

        }




        [Fact]
        public void ShouldLogDebugToUnitTestConsole()
        {
            var logger = BaseDependanciesTest.GetLogger<object>();
            logger.Warning("This is a warning string");
            logger.Debug("This is a debug string");
            logger.Information("This is an information string");
            logger.Error("This is a error string");
            logger.Fatal("This is a fatal string");
        }
    }
}
