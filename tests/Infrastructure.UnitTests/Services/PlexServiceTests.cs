using NUnit.Framework;
using PlexRipper.Domain.ValueObjects;
using PlexRipper.Infrastructure.Services;
using System.Threading.Tasks;

namespace Infrastructure.UnitTests.Services
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task ShouldReturnValidApiToken()
        {
            PlexService plexService = new PlexService();
            PlexAccount result = await plexService.RequestTokenAsync("", "");
            Assert.IsNotNull(result);
        }
    }
}
