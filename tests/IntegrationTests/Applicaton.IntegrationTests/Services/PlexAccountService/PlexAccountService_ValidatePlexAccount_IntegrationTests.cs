using System.Threading.Tasks;
using Logging;
using PlexRipper.BaseTests;
using Serilog.Events;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests
{
    public class PlexAccountService_ValidatePlexAccount_IntegrationTests
    {
        private BaseContainer Container { get; }

        public PlexAccountService_ValidatePlexAccount_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output, LogEventLevel.Verbose);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task Should_When()
        {
            // Arrange
            var plexAccount = Secrets.PlexAccount3;
            plexAccount.ClientId = Container.GetPlexAccountService.GeneratePlexAccountClientId().Value;

            // Act
            var result = await Container.GetPlexAccountService.ValidatePlexAccountAsync(plexAccount);

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }
    }
}