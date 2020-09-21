using System.Linq;
using System.Threading.Tasks;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Fixtures;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using Xunit.Priority;

namespace PlexRipper.Application.IntegrationTests.Services
{
    [Collection("PlexLibrary")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class PlexLibraryServiceIntegrationTests : IClassFixture<DatabaseFixture>
    {
        private BaseContainer Container { get; }

        public PlexLibraryServiceIntegrationTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = fixture.Container;
        }

        [Fact, Priority(1)]
        public async Task CreatePlexAccountAndRequestLibrary_ShouldReturnValidLibrary()
        {
            // Arrange
            var accountService = Container.GetPlexAccountService;
            var plexLibraryService = Container.GetPlexLibraryService;
            var credentials = Secrets.Account2;

            // Act
            var newAccount = new PlexAccount
            {
                DisplayName = "Test Account 1",
                Username = credentials.Username,
                Password = credentials.Password,
            };

            // Act
            var result = await accountService.CreatePlexAccountAsync(newAccount);
            result.IsFailed.ShouldBeFalse();

            // Retrieve account with included PlexServers and PlexLibraries
            result = await accountService.GetPlexAccountAsync(result.Value.Id);

            // This is very specific to the plex account used
            var library = GetLibraryFromPlexAccount(result.Value);
            library.ShouldNotBeNull();

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

        [Fact, Priority(2)]
        public async Task RequestMetadataForPlexLibrary_ShouldReturnValidMediaList()
        {
            // Arrange
            var accountService = Container.GetPlexAccountService;
            var plexLibraryService = Container.GetPlexLibraryService;

            var account = await accountService.GetPlexAccountAsync(1);
            var plexLibraryPick = GetLibraryFromPlexAccount(account.Value);

            // Refresh library
            var plexLibrary = await plexLibraryService.GetPlexLibraryAsync(plexLibraryPick.Id, account.Value.Id);
            plexLibrary = await plexLibraryService.RefreshLibraryMediaAsync(account.Value, plexLibraryPick);

            account.Value.ShouldNotBeNull();
            plexLibrary.Value.HasMedia.ShouldBeTrue();
        }

        private PlexLibrary GetLibraryFromPlexAccount(PlexAccount plexAccount)
        {
            // Retrieve the movies library
            return plexAccount.PlexServers[1]?.PlexLibraries?.Find(x => x.Title == "Movies") ?? null;
        }
    }
}