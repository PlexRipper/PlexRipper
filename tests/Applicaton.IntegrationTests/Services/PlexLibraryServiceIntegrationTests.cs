using System.Threading.Tasks;
using FluentAssertions;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Fixtures;
using PlexRipper.Domain;
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

        [Fact]
        [Priority(1)]
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
            result.IsFailed.Should().BeFalse();

            // Retrieve account with included PlexServers and PlexLibraries
            result = await accountService.GetPlexAccountAsync(result.Value.Id);

            // This is very specific to the plex account used
            var library = GetLibraryFromPlexAccount(result.Value);
            library.Should().NotBeNull();

            var plexLibrary = await plexLibraryService.GetPlexLibraryAsync(library.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.DisplayName.Should().Be(newAccount.DisplayName);
            result.Value.Username.Should().Be(newAccount.Username);
            result.Value.Password.Should().Be(newAccount.Password);
            result.Value.PlexAccountServers.Should().NotBeEmpty();
            result.Value.AuthenticationToken.Should().NotBeEmpty();

            plexLibrary.Value.Movies.Should().NotBeEmpty();
            plexLibrary.Value.PlexServer.Should().NotBeNull();
            plexLibrary.Value.HasMedia.Should().BeTrue();
        }

        [Fact]
        [Priority(2)]
        public async Task RequestMetadataForPlexLibrary_ShouldReturnValidMediaList()
        {
            // Arrange
            var accountService = Container.GetPlexAccountService;
            var plexLibraryService = Container.GetPlexLibraryService;

            var account = await accountService.GetPlexAccountAsync(1);
            var plexLibraryPick = GetLibraryFromPlexAccount(account.Value);

            // Refresh library
            var plexLibrary = await plexLibraryService.GetPlexLibraryAsync(plexLibraryPick.Id);
            plexLibrary = await plexLibraryService.RefreshLibraryMediaAsync(plexLibraryPick.Id);

            account.Value.Should().NotBeNull();
            plexLibrary.Value.HasMedia.Should().BeTrue();
        }

        private PlexLibrary GetLibraryFromPlexAccount(PlexAccount plexAccount)
        {
            // Retrieve the movies library
            return plexAccount.PlexServers[1]?.PlexLibraries?.Find(x => x.Title == "Movies") ?? null;
        }
    }
}