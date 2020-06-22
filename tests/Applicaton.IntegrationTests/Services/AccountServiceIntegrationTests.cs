using PlexRipper.BaseTests;
using PlexRipper.Domain.Entities;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests.Services
{
    public class AccountServiceIntegrationTests
    {
        private BaseContainer Container { get; }

        public AccountServiceIntegrationTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.Setup(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task FullAccountServiceIntegrationTest()
        {
            var test1 = await SetupAccount_ShouldReturnValidPlexAccount_WhenCredentialsAreValid();

            test1.ShouldBeTrue();
        }


        [Fact]
        public async Task<bool> SetupAccount_ShouldReturnValidPlexAccount_WhenCredentialsAreValid()
        {

            // Arrange
            var credentials = Secrets.GetCredentials();
            var accountService = Container.GetAccountService;
            var plexServerService = Container.GetPlexServerService;

            var newAccount = new Account
            {
                Username = credentials.Username,
                Password = credentials.Password
            };

            //Act 
            var isValid = await accountService.ValidateAccountAsync(newAccount);
            var accountDB = await accountService.CreateAccountAsync(newAccount);
            var serversList = await accountService.GetServersAsync(accountDB.Id);
            var plexServer = await plexServerService.GetAllLibraryMediaAsync(serversList.First(), true);

            //Assert
            isValid.ShouldBeTrue();
            accountDB.IsValidated.ShouldBeTrue();
            accountDB.PlexAccount.ShouldNotBeNull();
            serversList.ShouldNotBeEmpty();
            plexServer.PlexLibraries.ShouldNotBeEmpty();
            return true;
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
        public async Task ShouldReturnPlexLibrary()
        {

            var plexService = Container.GetPlexService;
            var accountService = Container.GetAccountService;
            var credentials = Secrets.GetCredentials();

            await accountService.CreateAccountAsync(new Account(credentials.Username, credentials.Password));
            var account = await accountService.GetAccountAsync(credentials.Username);

            var serverList = await plexService.GetServersAsync(account.PlexAccount);
            //var library = await plexService.GetLibrary(serverList[0]);

            //library.ShouldNotBeNull();

        }

    }
}
