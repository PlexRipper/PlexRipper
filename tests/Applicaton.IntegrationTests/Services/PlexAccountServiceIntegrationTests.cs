using PlexRipper.BaseTests;
using System.Threading.Tasks;
using FluentAssertions;
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
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.DisplayName.Should().Be(newAccount.DisplayName);
            result.Value.Username.Should().Be(newAccount.Username);
            result.Value.Password.Should().Be(newAccount.Password);
            result.Value.PlexAccountServers.Should().NotBeEmpty();
            result.Value.AuthenticationToken.Should().NotBeEmpty();
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

            var updateResult = await accountService.UpdatePlexAccountAsync(createResult.Value);

            // Assert
            createResult.IsSuccess.Should().BeTrue();
            createResult.Value.Should().NotBeNull();
            createResult.Value.Username.Should().Be(newAccount.Username);
            createResult.Value.Password.Should().Be(newAccount.Password);
            createResult.Value.PlexAccountServers.Should().NotBeEmpty();
            createResult.Value.AuthenticationToken.Should().NotBeEmpty();

            updateResult.IsSuccess.Should().BeTrue();
            updateResult.Value.Should().NotBeNull();
            updateResult.Value.DisplayName.Should().Be("Updated Test Account 999");
            updateResult.Value.Username.Should().Be(createResult.Value.Username);
            updateResult.Value.Password.Should().Be(createResult.Value.Password);
            updateResult.Value.PlexAccountServers.Should().NotBeEmpty();
            updateResult.Value.AuthenticationToken.Should().NotBeEmpty();

        }

    }
}
