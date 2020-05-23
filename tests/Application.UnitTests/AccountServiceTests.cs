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
        private BaseContainer Container { get; }


        public AccountServiceTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.Setup(output);
            Container = new BaseContainer();

        }

        [Fact, Order(1)]
        public async Task CreateAccountAsync_ShouldReturnValidAccount_WhenAccountDoesNotExist()
        {
            // Arrange
            var accountService = Container.GetAccountService;
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
            var accountService = Container.GetAccountService;
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
            var accountService = Container.GetAccountService;
            var newAccount = new Account("TestUsername", "Password123");
            var updatedAccount = new Account("TestUsername", "123PassWrd123");

            // Act
            var accountDB = await accountService.CreateAccountAsync(newAccount);
            updatedAccount.Id = accountDB.Id;
            accountDB = await accountService.UpdateAccountAsync(updatedAccount);

            // Assert
            accountDB.ShouldNotBeNull();
            accountDB.Password.ShouldBe(updatedAccount.Password);
        }


    }
}
