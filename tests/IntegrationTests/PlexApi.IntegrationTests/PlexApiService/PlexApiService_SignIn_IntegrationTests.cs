using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using PlexRipper.Application.Common;
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
            var plexAccount = new PlexAccount(Secrets.Account1.Username, Secrets.Account1.Password);

            // Act
            var signInResult = await plexApiService.PlexSignInAsync(plexAccount.ClientId, plexAccount.Username, plexAccount.Password);

            // Assert
            signInResult.IsSuccess.ShouldBeTrue();
            signInResult.Has201CreatedRequestSuccess().ShouldBeTrue();
        }

        [Fact]
        public async Task PlexApiService_SignIn_ShouldReturnAnUnauthorizedStatus_WhenAccountGivenWith2FAButWithoutVerificationCode()
        {
            // Arrange
            var plexApiService = Container.GetPlexApiService;
            var plexAccount = new PlexAccount(Secrets.Account2.Username, Secrets.Account2.Password);

            // Act
            var authPin = await plexApiService.GetPin();
            authPin.IsSuccess.ShouldBeTrue();

            // Result<AuthPin> authResult;
            // while (true)
            // {
            //     await Task.Delay(1000);
            //     authResult = await plexApiService.CheckPin(authPin.Value.Id, authPin.Value.Code, authPin.Value.ClientIdentifier);
            //     if (authResult.IsSuccess)
            //     {
            //         break;
            //     }
            // }
            //
            // var userInput = 235256;

            var signInResult = await plexApiService.PlexSignInAsync(plexAccount.ClientId, plexAccount.Username, plexAccount.Password);

            if (signInResult.HasError<PlexError>())
            {
                List<PlexError> errors = signInResult.Errors.OfType<PlexError>().ToList();
                // If the message is "Please enter the verification code" then 2FA is enabled.
                var accountHas2FA = errors.Any(x => x.Code == 1029);
            }

            // Assert
            signInResult.IsSuccess.ShouldBeFalse();
            signInResult.Has401UnauthorizedError().ShouldBeTrue();
        }
    }
}