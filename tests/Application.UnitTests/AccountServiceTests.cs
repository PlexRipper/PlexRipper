using PlexRipper.BaseTests;
using System.Threading.Tasks;
using FluentAssertions;
using PlexRipper.Domain;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.UnitTests
{
    public class AccountServiceTests
    {
        private BaseContainer Container { get; }


        public AccountServiceTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();

        }

        [Fact]
        public async Task CreateAccountAsync_ShouldReturnValidAccount_WhenAccountDoesNotExist()
        {
            // Arrange
            var accountService = Container.GetPlexAccountService;
            var newAccount = new PlexAccount("TestUsername", "Password123");

            // Act
            var account = await accountService.CreatePlexAccountAsync(newAccount);

            // Assert
            account.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldReturnNull_WhenAccountExist()
        {
            // Arrange
            var accountService = Container.GetPlexAccountService;
            var newAccount = new PlexAccount("TestUsername", "Password123");

            // Act
            var account = await accountService.CreatePlexAccountAsync(newAccount);
            var account2 = await accountService.CreatePlexAccountAsync(newAccount);

            // Assert
            account.Should().NotBeNull();
            account2.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAccountAsync_ShouldReturnValidAccount_WhenAccountWasUpdated()
        {
            // Arrange
            var accountService = Container.GetPlexAccountService;
            var newAccount = new PlexAccount("TestUsername", "Password123");
            var updatedAccount = new PlexAccount("TestUsername", "123PassWrd123");

            // Act
            var accountDB = (await accountService.CreatePlexAccountAsync(newAccount)).Value;
            updatedAccount.Id = accountDB.Id;
            accountDB = (await accountService.UpdatePlexAccountAsync(updatedAccount)).Value;

            // Assert
            accountDB.Should().NotBeNull();
            accountDB.Password.Should().Be(updatedAccount.Password);
        }


    }
}
