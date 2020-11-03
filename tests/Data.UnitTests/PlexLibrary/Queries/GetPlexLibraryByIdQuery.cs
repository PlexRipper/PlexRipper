using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using PlexRipper.Application.PlexLibraries;
using PlexRipper.BaseTests;
using PlexRipper.Data;
using PlexRipper.Domain;
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

            _dbContext.PlexServers.Add(FakeDbData.GetPlexServer());
            var plexLibrary = FakeDbData.GetPlexLibrary(1, 1, PlexMediaType.TvShow, _numberOfTvShow).Generate();
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
            result.IsFailed.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value.PlexServer.Should().NotBeNull();
            result.Value.TvShows.Should().NotBeEmpty();
            result.Value.TvShows.Count.Should().Be(_numberOfTvShow);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}