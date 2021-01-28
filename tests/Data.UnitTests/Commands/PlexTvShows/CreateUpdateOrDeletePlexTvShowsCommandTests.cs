using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.BaseTests;
using PlexRipper.Data;
using PlexRipper.Data.CQRS.PlexTvShows;
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
            var createCommand = new CreateUpdateOrDeletePlexTvShowsCommand(plexLibrary);
            var createCommandHandler = new CreateUpdateOrDeletePlexTvShowsCommandHandler(_dbContext);
            var createResult = await createCommandHandler.Handle(createCommand, new CancellationToken());

            var command = new GetPlexTvShowsByPlexLibraryIdQuery(plexLibrary.Id);
            var handler = new GetPlexTvShowsByPlexLibraryIdQueryHandler(_dbContext);
            var dbResult = await handler.Handle(command, new CancellationToken());

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

            Log.Information("CreateUpdateOrDeletePlexTvShowsCommand_CreateTvShows finished successfully!");
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

            var command = new GetPlexTvShowsByPlexLibraryIdQuery(plexLibrary.Id);
            var handler = new GetPlexTvShowsByPlexLibraryIdQueryHandler(_dbContext);
            var dbResult = await handler.Handle(command, new CancellationToken());

            // Update some tvShows, Seasons and episodes.
            for (int i = 0; i < 10; i++)
            {
                dbResult.Value[i].Title += " Updated!";
                dbResult.Value[i].UpdatedAt = dbResult.Value[i].UpdatedAt.AddDays(4);
                for (int j = 0; j < 5; j++)
                {
                    var tvShowSeason = dbResult.Value[i].Seasons[j];

                    tvShowSeason.Title += " Updated!";
                    tvShowSeason.UpdatedAt = tvShowSeason.UpdatedAt.AddDays(4);

                    for (int k = 0; k < 5; k++)
                    {
                        var episode = tvShowSeason.Episodes[k];
                        episode.Title += " Updated!";
                        episode.UpdatedAt = episode.UpdatedAt.AddDays(4);
                    }
                }
            }

            // Change values
            plexLibrary.TvShows = dbResult.Value;

            /*
             * Act
             */
            var updatedResult = await _mediator.Send(new CreateUpdateOrDeletePlexTvShowsCommand(plexLibrary));

            var dbUpdateResult = await handler.Handle(command, new CancellationToken());

            /*
             * Assert
             */
            createResult.IsFailed.Should().BeFalse();
            createResult.Value.Should().BeTrue();
            updatedResult.IsFailed.Should().BeFalse();
            updatedResult.Value.Should().BeTrue();

            dbResult.Value.Count.Should().Be(_numberOfTvShow);

            dbUpdateResult.Value.Count.Should().Be(_numberOfTvShow);
            for (int i = 0; i < 10; i++)
            {
                var plexTvShow = dbUpdateResult.Value[i];
                plexTvShow.Should().NotBeNull();
                plexTvShow.Title.Should().NotBeEmpty();
                plexTvShow.Key.Should().BeGreaterThan(0);
                plexTvShow.Id.Should().BeGreaterThan(0);
                plexTvShow.Title.Contains("Updated!").Should().BeTrue($"TvShow: {plexTvShow.Id}");
                plexTvShow.Seasons.Count.Should().BeGreaterThan(5);

                for (int j = 0; j < 5; j++)
                {
                    var tvShowSeason = plexTvShow.Seasons[j];

                    tvShowSeason.Episodes.Count.Should().BeGreaterThan(5);
                    tvShowSeason.Title.Contains("Updated!").Should().BeTrue($"TvShowSeason: {tvShowSeason.Id}");

                    for (int k = 0; k < 5; k++)
                    {
                        var episode = tvShowSeason.Episodes[k];
                        episode.Title.Contains("Updated!").Should().BeTrue($"TvShowEpisode: {episode.Id}");
                    }
                }
            }

            Log.Information("CreateUpdateOrDeletePlexTvShowsCommand_UpdateTvShows finished successfully!");
        }

        [Fact]
        public async Task CreateUpdateOrDeletePlexTvShowsCommand_DeleteTvShows()
        {
            // Arrange
            SetupDatabase();
            var plexLibrary = FakeDbData.GetPlexLibrary(1, 1, PlexMediaType.TvShow, _numberOfTvShow).Generate();

            var createCommandHandler = new CreateUpdateOrDeletePlexTvShowsCommandHandler(_dbContext);
            var createResult = await createCommandHandler.Handle(new CreateUpdateOrDeletePlexTvShowsCommand(plexLibrary), new CancellationToken());

            // Act
            var handler = new GetPlexTvShowsByPlexLibraryIdQueryHandler(_dbContext);
            var dbResult = await handler.Handle(new GetPlexTvShowsByPlexLibraryIdQuery(plexLibrary.Id), new CancellationToken());

            dbResult.Value.RemoveRange(10, 10);
            var seasonCount = dbResult.Value.SelectMany(x => x.Seasons).Count();
            var episodeCount = dbResult.Value.SelectMany(x => x.Seasons.Select(episode => episode.Episodes)).Count();
            plexLibrary.TvShows = dbResult.Value;

            var updatedResult = await _mediator.Send(new CreateUpdateOrDeletePlexTvShowsCommand(plexLibrary));
            var dbUpdateResult = await handler.Handle(new GetPlexTvShowsByPlexLibraryIdQuery(plexLibrary.Id), new CancellationToken());

            // Assert
            createResult.IsFailed.Should().BeFalse();
            updatedResult.IsFailed.Should().BeFalse();
            createResult.Value.Should().BeTrue();
            updatedResult.Value.Should().BeTrue();

            dbUpdateResult.Value.Count.Should().Be(_numberOfTvShow - 10);
            dbUpdateResult.Value.SelectMany(x => x.Seasons).Count().Should().Be(seasonCount);
            dbUpdateResult.Value.SelectMany(x => x.Seasons.Select(episode => episode.Episodes)).Count().Should().Be(episodeCount);

            foreach (var plexTvShow in dbResult.Value)
            {
                plexTvShow.Should().NotBeNull();
                plexTvShow.Title.Should().NotBeEmpty();
                plexTvShow.Key.Should().BeGreaterThan(0);
                plexTvShow.Id.Should().BeGreaterThan(0);
            }

            Log.Information("CreateUpdateOrDeletePlexTvShowsCommand_DeleteTvShows finished successfully!");
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}