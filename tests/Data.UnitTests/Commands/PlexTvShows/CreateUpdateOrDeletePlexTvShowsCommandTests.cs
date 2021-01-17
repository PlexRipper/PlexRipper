using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.BaseTests;
using PlexRipper.Data;
using PlexRipper.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Data.UnitTests.Commands
{
    [Collection("CreateUpdateOrDeletePlexTvShowsCommandTests")]
    public class CreateUpdateOrDeletePlexTvShowsCommandTests : IDisposable
    {
        private BaseContainer Container { get; }

        private PlexRipperDbContext _dbContext { get; }

        private IMediator _mediator { get; }

        private const int _numberOfTvShow = 50;

        private int _numberOfTvShowHalf = (int)Math.Floor(_numberOfTvShow / 2D);

        public CreateUpdateOrDeletePlexTvShowsCommandTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();
            _dbContext = Container.PlexRipperDbContext;
            _mediator = Container.Mediator;
        }

        private void SetupDatabase()
        {
            PlexServer plexServer = FakeDbData.GetPlexServer().Generate();
            PlexLibrary plexLibrary = FakeDbData.GetPlexLibrary(1, 1, PlexMediaType.TvShow).Generate();
            try
            {
                _dbContext.PlexServers.Add(plexServer);
                _dbContext.PlexLibraries.Add(plexLibrary);
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                throw;
            }
        }

        [Fact]
        public async Task CreateUpdateOrDeletePlexTvShowsCommand_CreateTvShows()
        {
            // Arrange
            SetupDatabase();
            var plexLibrary = FakeDbData.GetPlexLibrary(1, 1, PlexMediaType.TvShow, _numberOfTvShow).Generate();

            // Act
            var createResult =
                await _mediator.Send(new CreateUpdateOrDeletePlexTvShowsCommand(plexLibrary));
            var dbResult = await _mediator.Send(new GetPlexTvShowsByPlexLibraryIdQuery(plexLibrary.Id));

            // Assert
            createResult.IsFailed.Should().BeFalse();
            createResult.Value.Should().BeTrue();

            dbResult.Value.Count.Should().Be(_numberOfTvShow);
            foreach (var plexTvShow in dbResult.Value)
            {
                plexTvShow.Should().NotBeNull();
                plexTvShow.Title.Should().NotBeEmpty();
                plexTvShow.Key.Should().BeGreaterThan(0);
                plexTvShow.Id.Should().BeGreaterThan(0);
            }
        }

        [Fact]
        public async Task CreateUpdateOrDeletePlexTvShowsCommand_UpdateTvShows()
        {
            /*
             * Arrange
             */
            SetupDatabase();
            var plexLibrary = FakeDbData.GetPlexLibrary(1, 1, PlexMediaType.TvShow, _numberOfTvShow).Generate();

            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexTvShowsCommand(plexLibrary));
            var dbResult = await _mediator.Send(new GetPlexTvShowsByPlexLibraryIdQuery(plexLibrary.Id));

            // Change values
            var updatePlexTvShows = dbResult.Value.Select(x =>
            {
                if (x.Id % 2 == 0)
                {
                    x.Title += " Updated!";
                    x.UpdatedAt = x.UpdatedAt.AddDays(4);

                    // Change every even tvShow to mimic updated data
                    foreach (var tvShowSeason in x.Seasons)
                    {
                        if (tvShowSeason.Id % 2 == 0)
                        {
                            tvShowSeason.Title += " Updated Season!";
                            tvShowSeason.UpdatedAt = tvShowSeason.UpdatedAt.AddDays(4);
                            foreach (var episode in tvShowSeason.Episodes)
                            {
                                if (episode.Id % 2 == 0)
                                {
                                    episode.Title += " Updated Episode!";
                                    episode.UpdatedAt = episode.UpdatedAt.AddDays(4);
                                }
                            }
                        }
                    }
                }

                return x;
            }).ToList();

            // Add some new TvShows
            updatePlexTvShows.AddRange(FakeDbData.GetPlexTvShows(1).Generate(20));
            plexLibrary.TvShows = updatePlexTvShows;

            /*
             * Act
             */
            var updatedResult = await _mediator.Send(new CreateUpdateOrDeletePlexTvShowsCommand(plexLibrary));
            var dbUpdateResult = await _mediator.Send(new GetPlexTvShowsByPlexLibraryIdQuery(plexLibrary.Id));

            /*
             * Assert
             */
            createResult.IsFailed.Should().BeFalse();
            createResult.Value.Should().BeTrue();
            updatedResult.IsFailed.Should().BeFalse();
            updatedResult.Value.Should().BeTrue();

            dbResult.Value.Count.Should().Be(_numberOfTvShow);

            dbUpdateResult.Value.Count.Should().Be(_numberOfTvShow);
            foreach (var plexTvShow in dbUpdateResult.Value)
            {
                plexTvShow.Should().NotBeNull();
                plexTvShow.Title.Should().NotBeEmpty();
                plexTvShow.Key.Should().BeGreaterThan(0);
                plexTvShow.Id.Should().BeGreaterThan(0);
                if (plexTvShow.Id % 2 == 0)
                {
                    plexTvShow.Title.Contains("Updated!").Should().BeTrue();
                }
            }
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}