using PlexRipper.BaseTests;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace PlexApi.UnitTests
{
    public class PlexApiServiceTests
    {
        private BaseContainer Container { get; }

        public PlexApiServiceTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task ShouldReturnValidPlexAccountDTO_WhenCredentialsAreValid()
        {
            // Arrange
            var credentials = Secrets.Account1;
            var plexApiService = Container.GetPlexApiService;

            // Act
            var plexAccount = await plexApiService.PlexSignInAsync(credentials.Username, credentials.Password);

            // Assert
            plexAccount.Should().NotBeNull();
        }

      }
}
