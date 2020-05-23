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
        private BaseContainer Container { get; }

        public PlexServiceIntegrationTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.Setup(output);
            Container = new BaseContainer();

        }


        [Fact]
        public async Task GetPlexToken_ShouldReturnValidApiToken()
        {
            using (var mock = AutoMock.GetLoose())
            {

                // Arrange
                var plexService = mock.Create<PlexService>();
                var accountService = Container.GetAccountService;
                var credentials = Secrets.GetCredentials();

                //Act 
                var newAccount = new Account
                {
                    Username = credentials.Username,
                    Password = credentials.Password
                };

                var result = await accountService.ValidateAccountAsync(newAccount);
                var account = await accountService.CreateAccountAsync(newAccount);

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
            var plexService = Container.GetPlexService;
            var accountService = Container.GetAccountService;
            var credentials = Secrets.GetCredentials();

            //Act 
            var account = await accountService.CreateAccountAsync(new Account
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
