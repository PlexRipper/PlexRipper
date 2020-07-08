using PlexRipper.BaseTests;
using PlexRipper.Domain.Entities;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests.Services
{
    public class PlexLibraryServiceIntegrationTests
    {
        private BaseContainer Container { get; }

        public PlexLibraryServiceIntegrationTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.Setup(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task CreatePlexAccountAndRequestLibrary_ShouldReturnValidLibrary()
        {
            // Arrange
            var accountService = Container.GetPlexAccountService;
            var plexLibraryService = Container.GetPlexLibraryService;
            var credentials = Secrets.GetCredentials();

            //Act 
            var newAccount = new PlexAccount
            {
                DisplayName = "Test Account 1",
                Username = credentials.Username,
                Password = credentials.Password
            };


            // Act
            var result = await accountService.CreatePlexAccountAsync(newAccount);
            var library = result.Value.PlexServers.First().PlexLibraries.ToList()[4];
            var plexLibrary = await plexLibraryService.GetPlexLibraryAsync(library.Id, result.Value.Id);


            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.DisplayName.ShouldBe(newAccount.DisplayName);
            result.Value.Username.ShouldBe(newAccount.Username);
            result.Value.Password.ShouldBe(newAccount.Password);
            result.Value.PlexAccountServers.ShouldNotBeEmpty();
            result.Value.AuthenticationToken.ShouldNotBeEmpty();

            plexLibrary.Value.Movies.ShouldNotBeEmpty();
            plexLibrary.Value.PlexServer.ShouldNotBeNull();
            plexLibrary.Value.HasMedia.ShouldBeTrue();
        }
    }
}
