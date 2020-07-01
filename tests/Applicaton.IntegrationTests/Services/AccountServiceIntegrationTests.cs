using PlexRipper.BaseTests;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests.Services
{
    public class AccountServiceIntegrationTests
    {
        private BaseContainer Container { get; }

        public AccountServiceIntegrationTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.Setup(output);
            Container = new BaseContainer();
        }

    }
}
