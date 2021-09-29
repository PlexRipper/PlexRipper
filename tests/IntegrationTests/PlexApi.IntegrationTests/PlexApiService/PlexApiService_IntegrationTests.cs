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

            // Act
            var authPin = await plexApiService.GetPin();
            authPin.IsSuccess.ShouldBeTrue();

            Result<AuthPin> authResult;
            while (true)
            {
                await Task.Delay(1000);
                authResult = await plexApiService.CheckPin(authPin.Value.Id, authPin.Value.Code, authPin.Value.ClientIdentifier);
                if (authResult.IsSuccess)
                {
                    break;
                }
            }

            // Assert
            authPin.IsSuccess.ShouldBeTrue();
            authResult.IsSuccess.ShouldBeTrue();
        }
    }
}