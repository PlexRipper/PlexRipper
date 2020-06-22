using PlexRipper.Application.IntegrationTests.Base;
using PlexRipper.Domain.Entities;
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
            var plexDownloadService = Container.GetPlexDownloadService;
            var accountService = Container.GetAccountService;
            var plexServerService = Container.GetPlexServerService;
            var credentials = Secrets.GetCredentials();

            //Act 
            var newAccount = new Account
            {
                Username = credentials.Username,
                Password = credentials.Password
            };

            var result = await accountService.ValidateAccountAsync(newAccount);
            var account = await accountService.CreateAccountAsync(newAccount);
            var serverList = await plexServerService.GetServersAsync(account.PlexAccount);
            var testServer = serverList.First();

            //var downloadRequest = new DownloadRequest();
            //plexDownloadService.StartDownload(downloadRequest);

            //Assert

        }
    }
}
