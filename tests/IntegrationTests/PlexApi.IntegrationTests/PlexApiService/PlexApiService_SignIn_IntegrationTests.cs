using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Serilog.Events;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexApi.IntegrationTests
{
    public class PlexApiService_SignIn_IntegrationTests
    {
        private BaseContainer Container { get; }

        public PlexApiService_SignIn_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output, LogEventLevel.Verbose);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task PlexApiService_SignIn_ShouldReturnAnOkStatus_WhenValidAccountGivenWithout2FA()
        {
            // Arrange
            var plexApiService = Container.GetPlexApiService;
            var plexAccount = new PlexAccount(Secrets.Account1.Username, Secrets.Account1.Password, "AABBCCDDEERRFFGWEG");


            // Act
            var signInResult = await plexApiService.PlexSignInAsync(plexAccount);

            // Assert
            signInResult.IsSuccess.ShouldBeTrue();
            signInResult.Has201CreatedRequestSuccess().ShouldBeTrue();
        }

        [Fact]
        public async Task PlexApiService_SignIn_ShouldReturnAnUnauthorizedStatus_WhenAccountGivenWith2FAButWithoutVerificationCode()
        {
            // Arrange
            var plexApiService = Container.GetPlexApiService;
            var plexAccount = Secrets.PlexAccount2;

            // Act
            var signInResult = await plexApiService.PlexSignInAsync(plexAccount);

            // Assert
            signInResult.IsSuccess.ShouldBeFalse();
            signInResult.Has401UnauthorizedError().ShouldBeTrue();
            signInResult.Errors.OfType<PlexError>().ToList().Any(x => x.Code == 1029).ShouldBeTrue();
        }
    }
}