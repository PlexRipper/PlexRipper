using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using PlexRipper.Application.Common;
using PlexRipper.BaseTests;
using Serilog.Events;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexApi.IntegrationTests
{
    public class PlexApiService
    {
        private BaseContainer Container { get; }

        public PlexApiService(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output, LogEventLevel.Verbose);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task GetPin_ShouldRetrieveAValidPin_WhenRequested()
        {
            // Arrange
            var plexApiService = Container.GetPlexApiService;
            var plexAccount = Secrets.PlexAccount2;

            // Act
            var authPin = await plexApiService.Get2FAPin(plexAccount.ClientId);

            // Assert
            authPin.IsSuccess.ShouldBeTrue();
            authPin.Value.Code.ShouldNotBeEmpty();
            authPin.Value.Id.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task CheckPin_ShouldRetrieveAValidCheckPin_WhenTheSameClientIdIsGiven()
        {
            // Arrange
            var plexApiService = Container.GetPlexApiService;
            var plexAccount = Secrets.PlexAccount2;

            // Act
            var getAuthPin = await plexApiService.Get2FAPin(plexAccount.ClientId);
            getAuthPin.IsSuccess.ShouldBeTrue();

            await Task.Delay(1000);
            var checkAuthPin = await plexApiService.Check2FAPin(getAuthPin.Value.Id, getAuthPin.Value.ClientIdentifier);

            checkAuthPin.IsSuccess.ShouldBeTrue();
            getAuthPin.Value.ClientIdentifier.ShouldBe(checkAuthPin.Value.ClientIdentifier);
        }
    }
}