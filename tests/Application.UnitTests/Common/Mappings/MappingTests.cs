using AutoMapper;
using PlexRipper.Application.Config.Mappings;
using Xunit;

namespace PlexRipper.Application.UnitTests.Common.Mappings
{
    public class MappingTests
    {
        private readonly IConfigurationProvider _configuration;

        public MappingTests()
        {
            _configuration = new MapperConfiguration(cfg => { cfg.AddProfile<ApplicationMappingProfile>(); });
        }

        [Fact]
        public void ShouldHaveValidConfiguration()
        {
            _configuration.AssertConfigurationIsValid();
        }
    }
}