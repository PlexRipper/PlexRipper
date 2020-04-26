using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PlexRipper.Application.Common.Mappings;
using PlexRipper.Infrastructure.Persistence;
using PlexRipper.Infrastructure.Services;
using System.Threading.Tasks;

namespace Infrastructure.UnitTests.Services
{
    public class PlexServiceTests
    {
        private PlexRipperDbContext _context;
        private Mapper _mapper;

        [SetUp]
        public void Setup()
        {
            // Setup DB
            var options = new DbContextOptionsBuilder<PlexRipperDbContext>()
                .UseSqlite("Data Source=PlexRipperDB_TESTS.db")
                .Options;
            PlexRipperDbContext context = new PlexRipperDbContext(options);
            // context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            _context = context;

            //Setup mapper
            var myProfile = new InfrastructureProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            _mapper = new Mapper(configuration);
        }

        [Test]
        public async Task ShouldReturnValidApiToken()
        {
            var plexService = new PlexService(_context, _mapper);
            var accountService = new AccountService(_context, _mapper, plexService);
            var account = accountService.AddAccount("", "");
            var result = await accountService.ValidateAccount(account);
            Assert.IsNotNull(result);

            string authToken = await plexService.GetPlexToken(account);
            Assert.IsNotEmpty(authToken);
        }
    }
}
