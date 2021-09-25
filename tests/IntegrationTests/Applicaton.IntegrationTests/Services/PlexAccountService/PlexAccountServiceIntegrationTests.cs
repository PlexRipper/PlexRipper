using System.Threading.Tasks;
using Logging;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests
{
    public class PlexAccountServiceIntegrationTests
    {
        private BaseContainer Container { get; }

        public PlexAccountServiceIntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
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
                Password = credentials.Password,
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
    }
}