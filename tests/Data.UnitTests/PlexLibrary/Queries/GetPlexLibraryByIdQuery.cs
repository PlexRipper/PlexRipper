using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MediatR;
using PlexRipper.Application.PlexLibraries;
using PlexRipper.BaseTests;
using PlexRipper.Data;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Data.UnitTests
{
    [Collection("GetPlexLibraryByIdQueryTests")]
    public class GetPlexLibraryByIdQueryTests : IDisposable
    {
        private BaseContainer Container { get; }

        private PlexRipperDbContext _dbContext { get; }

        private IMediator _mediator { get; }

        private const int _numberOfTvShow = 50;

        public GetPlexLibraryByIdQueryTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();
            _dbContext = Container.PlexRipperDbContext;
            _mediator = Container.Mediator;
        }

        private void SetupDatabase()
        {
            var sw = Stopwatch.StartNew();

            _dbContext.PlexServers.Add(FakeData.GetPlexServer());
            var plexLibrary = FakeData.GetPlexLibrary(1, 1, PlexMediaType.TvShow, _numberOfTvShow).Generate();
            _dbContext.PlexLibraries.Add(plexLibrary);
            _dbContext.SaveChanges();

            sw.Stop();
            Log.Debug($"Database was setup in {sw.ElapsedMilliseconds}");
        }

        [Fact]
        public async Task GetPlexLibraryByIdQuery_GetValidPlexLibraryWithIncludedServerAndMedia()
        {
            // Arrange
            SetupDatabase();

            // Act
            var result = await _mediator.Send(new GetPlexLibraryByIdQuery(1, true, true));

            // Assert
            result.IsFailed.ShouldBeFalse();
            result.Value.ShouldNotBeNull();
            result.Value.PlexServer.ShouldNotBeNull();
            result.Value.TvShows.ShouldNotBeEmpty();
            result.Value.TvShows.Count.ShouldBe(_numberOfTvShow);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}