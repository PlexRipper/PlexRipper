using PlexRipper.Application.IntegrationTests.Base;
using PlexRipper.Domain.Entities;
using Shouldly;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace PlexRipper.Application.UnitTests
{
    [Order(1)]
    public class AccountServiceTests
    {


        public AccountServiceTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.Setup(output);
        }

        [Fact, Order(1)]
        public async Task CreateAccountAsync_ShouldReturnValidAccount_WhenAccountDoesNotExist()
        {
            // Arrange
            var accountService = BaseServiceTest.GetAccountService();
            var newAccount = new Account("TestUsername", "Password123");

            // Act
            var account = await accountService.CreateAccountAsync(newAccount);

            // Assert
            account.ShouldNotBeNull();
        }

        [Fact, Order(2)]
        public async Task CreateAccountAsync_ShouldReturnNull_WhenAccountExist()
        {
            // Arrange
            var accountService = BaseServiceTest.GetAccountService();
            var newAccount = new Account("TestUsername", "Password123");

            // Act
            var account = await accountService.CreateAccountAsync(newAccount);
            var account2 = await accountService.CreateAccountAsync(newAccount);

            // Assert
            account.ShouldNotBeNull();
            account2.ShouldBeNull();
        }

        [Fact, Order(3)]
        public async Task UpdateAccountAsync_ShouldReturnValidAccount_WhenAccountWasUpdated()
        {
            // Arrange
            var accountService = BaseServiceTest.GetAccountService();
            var newAccount = new Account("TestUsername", "Password123");
            var updatedAccount = new Account("TestUsername", "123Password123");
            // Act
            var account = await accountService.CreateAccountAsync(newAccount);
            account = await accountService.UpdateAccountAsync(updatedAccount);

            // Assert
            account.ShouldNotBeNull();
        }


    }
}
