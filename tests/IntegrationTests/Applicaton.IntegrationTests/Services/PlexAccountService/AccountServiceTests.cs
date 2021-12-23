using System.Threading.Tasks;
using Logging;
using PlexRipper.Application.PlexAccounts;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests
{
    public class AccountServiceTests
    {
        private BaseContainer Container { get; }

        public AccountServiceTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
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
            account.ShouldNotBeNull();
        }

       // [Fact]
        // public async Task CreateAccountAsync_ShouldReturnNull_WhenAccountExist()
        // {
        //     // Arrange
        //     var accountService = Container.GetPlexAccountService;
        //     var newAccount = Container.PlexAccountMain;
        //
        //     // Act
        //     await accountService.CreatePlexAccountAsync(newAccount);
        //     var account = await accountService.CreatePlexAccountAsync(newAccount);
        //
        //     // Assert
        //     account.IsFailed.ShouldBeTrue();
        // }

        // [Fact]
        // public async Task UpdateAccountAsync_ShouldReturnValidAccount_WhenAccountWasUpdated()
        // {
        //     // Arrange
        //     var accountService = Container.GetPlexAccountService;
        //     var newAccount = Container.PlexAccountMain;
        //
        //     // Act
        //     var createAccountResult = await accountService.CreatePlexAccountAsync(newAccount);
        //     var dbResult = await Container.Mediator.Send(new GetPlexAccountByIdQuery(createAccountResult.Value.Id));
        //     if (dbResult.IsFailed)
        //     {
        //         dbResult.IsSuccess.ShouldBeTrue();
        //     }
        //
        //     var updatedAccountResult = await accountService.UpdatePlexAccountAsync(dbResult.Value);
        //
        //     // Assert
        //     createAccountResult.IsSuccess.ShouldBeTrue();
        //     createAccountResult.Value.ShouldNotBeNull();
        //
        //     updatedAccountResult.IsSuccess.ShouldBeTrue();
        //     updatedAccountResult.Value.Id.ShouldBe(dbResult.Value.Id);
        //     updatedAccountResult.Value.Email.ShouldBe(dbResult.Value.Email);
        //     updatedAccountResult.Value.Username.ShouldBe(dbResult.Value.Username);
        //     updatedAccountResult.Value.Password.ShouldBe(dbResult.Value.Password);
        // }
    }
}