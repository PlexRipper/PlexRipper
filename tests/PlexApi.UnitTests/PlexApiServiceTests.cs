using PlexRipper.BaseTests;
using Shouldly;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PlexApi.UnitTests
{
    public class PlexApiServiceTests
    {
        private BaseContainer Container { get; }

        public PlexApiServiceTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.Setup(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task ShouldReturnValidPlexAccountDTO_WhenCredentialsAreValid()
        {
            // Arrange
            var credentials = Secrets.GetCredentials();
            var plexApiService = Container.GetPlexApiService;

            // Act
            var plexAccount = await plexApiService.PlexSignInAsync(credentials.Username, credentials.Password);

            // Assert
            plexAccount.ShouldNotBeNull();
        }
    }
}
