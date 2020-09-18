using PlexRipper.BaseTests;
using Shouldly;
using System.Threading.Tasks;
using PlexRipper.Domain;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests.Services
{
    public class PlexAccountServiceIntegrationTests
    {
        private BaseContainer Container { get; }

        public PlexAccountServiceIntegrationTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task CreatePlexAccountAndShouldBeStoredInDB()
        {
            // Arrange
            var accountService = Container.GetPlexAccountService;
            var credentials = Secrets.Account1;

            //Act
            var newAccount = new PlexAccount
            {
                DisplayName = "Test Account 1",
                Username = credentials.Username,
                Password = credentials.Password
            };


            // Act
            var result = await accountService.CreatePlexAccountAsync(newAccount);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.DisplayName.ShouldBe(newAccount.DisplayName);
            result.Value.Username.ShouldBe(newAccount.Username);
            result.Value.Password.ShouldBe(newAccount.Password);
            result.Value.PlexAccountServers.ShouldNotBeEmpty();
            result.Value.AuthenticationToken.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task UpdatePlexAccountWithNewDisplayName()
        {
            // Arrange
            var accountService = Container.GetPlexAccountService;
            var credentials = Secrets.Account1;

            //Act
            var newAccount = new PlexAccount
            {
                DisplayName = "Test Account",
                Username = credentials.Username,
                Password = credentials.Password
            };

            // Act
            var createResult = await accountService.CreatePlexAccountAsync(newAccount);

            createResult.Value.DisplayName = "Updated Test Account 999";

            var updateResult = await accountService.UpdateAccountAsync(createResult.Value);

            // Assert
            createResult.IsSuccess.ShouldBeTrue();
            createResult.Value.ShouldNotBeNull();
            createResult.Value.Username.ShouldBe(newAccount.Username);
            createResult.Value.Password.ShouldBe(newAccount.Password);
            createResult.Value.PlexAccountServers.ShouldNotBeEmpty();
            createResult.Value.AuthenticationToken.ShouldNotBeEmpty();

            updateResult.IsSuccess.ShouldBeTrue();
            updateResult.Value.ShouldNotBeNull();
            updateResult.Value.DisplayName.ShouldBe("Updated Test Account 999");
            updateResult.Value.Username.ShouldBe(createResult.Value.Username);
            updateResult.Value.Password.ShouldBe(createResult.Value.Password);
            updateResult.Value.PlexAccountServers.ShouldNotBeEmpty();
            updateResult.Value.AuthenticationToken.ShouldNotBeEmpty();

        }

    }
}
