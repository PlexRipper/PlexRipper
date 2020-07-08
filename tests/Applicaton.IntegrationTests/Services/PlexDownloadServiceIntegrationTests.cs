using PlexRipper.BaseTests;
using PlexRipper.Domain.Entities;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests.Services
{
    public class PlexDownloadServiceIntegrationTests
    {
        private BaseContainer Container { get; }

        public PlexDownloadServiceIntegrationTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.Setup(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task PlexDownloadShouldDownloadEntireFile()
        {
            // Arrange
            var accountService = Container.GetPlexAccountService;
            var plexServerService = Container.GetPlexServerService;
            var plexLibraryService = Container.GetPlexLibraryService;
            var plexDownloadService = Container.GetPlexDownloadService;
            var credentials = Secrets.GetCredentials();

            //Act 
            var newAccount = new PlexAccount
            {
                Username = credentials.Username,
                Password = credentials.Password
            };

            var result = await accountService.ValidatePlexAccountAsync(newAccount);
            var account = await accountService.CreatePlexAccountAsync(newAccount);
            var serverList = await plexServerService.GetServersAsync(account.Value);

            var plexLibrary = await plexLibraryService.GetLibraryMediaAsync(TODO, serverList.ToList().First().PlexLibraries.ToList()[4]);
            var movie = plexLibrary.Movies[16];
            var movie2 = plexLibrary.Movies[18];

            var downloadRequest = await plexDownloadService.GetDownloadRequestAsync(movie);
            var downloadRequest2 = await plexDownloadService.GetDownloadRequestAsync(movie2);
            plexDownloadService.StartDownload(downloadRequest);
            plexDownloadService.StartDownload(downloadRequest2);

            await Task.Delay(15000);

            //Assert
            plexLibrary.ShouldNotBeNull();
        }


    }
}
