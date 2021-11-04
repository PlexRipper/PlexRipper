using System.Threading.Tasks;
using Logging;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Data.IntegrationTests
{
    public class CallHandler_IntegrationTests
    {
        private BaseContainer Container { get; }

        public CallHandler_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task ShouldHaveNoUpdates_WhenGivenAnEmptyList()
        {
            // Arrange
            var downloadTasks = FakeData.GetMovieDownloadTask().Generate(2);

            // Act
           var result = await Container.Mediator.Send(new UpdateDownloadTasksByIdCommand(downloadTasks));

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }
    }
}