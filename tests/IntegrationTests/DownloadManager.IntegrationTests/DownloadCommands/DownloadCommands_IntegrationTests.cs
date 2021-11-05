using Moq;
using Xunit;
using Xunit.Abstractions;
using Shouldly;
using Logging;
using Autofac.Extras.Moq;
using PlexRipper.BaseTests;
using System.Threading.Tasks;

namespace DownloadManager.IntegrationTests.DownloadCommands
{
    public class DownloadCommands_IntegrationTests
    {

        private BaseContainer Container { get; }

        public DownloadCommands_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task ShouldHaveNoUpdates_WhenGivenAnEmptyList()
        {
            // Arrange

            // Act

            // Assert

        }
    }
}