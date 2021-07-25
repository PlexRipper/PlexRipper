using System;
using System.Threading.Tasks;
using MediatR;
using PlexRipper.Application.PlexMovies;
using PlexRipper.BaseTests;
using PlexRipper.Data;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Data.UnitTests.Commands
{
    [Collection("PlexMoviesCommandTests")]
    public class CreateUpdateOrDeletePlexMoviesCommandTests : IDisposable
    {
        private BaseContainer Container { get; }

        private PlexRipperDbContext _dbContext { get; }

        private IMediator _mediator { get; }

        private const int _numberOfMovies = 100;

        private readonly int _numberOfMoviesHalf = (int)Math.Floor(_numberOfMovies / (double)2);

        public CreateUpdateOrDeletePlexMoviesCommandTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();
            _dbContext = Container.PlexRipperDbContext;
            _mediator = Container.Mediator;
        }

        private void SetupDatabase()
        {
            _dbContext.PlexServers.Add(FakeData.GetPlexServer());
            _dbContext.PlexLibraries.Add(FakeData.GetPlexLibrary(1, 1, PlexMediaType.TvShow).Generate());
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task CreateUpdateOrDeletePlexMoviesCommand_CreateMovies()
        {
            // Arrange
            SetupDatabase();
            var plexLibrary = FakeData.GetPlexLibrary(1, 1, PlexMediaType.Movie, _numberOfMovies).Generate();

            // Act
            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));
            var dbResult = await _mediator.Send(new GetPlexMoviesByPlexLibraryId(plexLibrary.Id));

            // Assert
            createResult.IsFailed.ShouldBeFalse();
            createResult.Value.ShouldBeTrue();

            dbResult.Value.Count.ShouldBe(_numberOfMovies);
            foreach (var plexMovie in dbResult.Value)
            {
                plexMovie.ShouldNotBeNull();
                plexMovie.Title.ShouldNotBeEmpty();
                plexMovie.Key.ShouldBeGreaterThan(0);
                plexMovie.Id.ShouldBeGreaterThan(0);
            }
        }

        [Fact]
        public async Task CreateUpdateOrDeletePlexMoviesCommand_UpdateMovies()
        {
            // Arrange
            SetupDatabase();
            var plexLibrary = FakeData.GetPlexLibrary(1, 1, PlexMediaType.Movie, _numberOfMovies).Generate();
            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));
            createResult.IsSuccess.ShouldBeTrue(createResult.Errors.ToString());

            var dbResult = await _mediator.Send(new GetPlexMoviesByPlexLibraryId(plexLibrary.Id));
            dbResult.IsSuccess.ShouldBeTrue();

            // change the plexMovies

            for (int i = 0; i < 50; i++)
            {
                dbResult.Value[i].Title += " Updated!";
                dbResult.Value[i].UpdatedAt = dbResult.Value[i].UpdatedAt.AddDays(4);
            }

            // Change values
            plexLibrary.Movies = dbResult.Value;

            // Act
            var updatedResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));
            updatedResult.IsSuccess.ShouldBeTrue();

            var dbUpdateResult = await _mediator.Send(new GetPlexMoviesByPlexLibraryId(plexLibrary.Id));
            dbUpdateResult.IsSuccess.ShouldBeTrue();

            // Assert
            createResult.Value.ShouldBeTrue();
            updatedResult.IsFailed.ShouldBeFalse();
            updatedResult.Value.ShouldBeTrue();

            dbUpdateResult.Value.Count.ShouldBe(_numberOfMovies);
            for (int i = 0; i < 50; i++)
            {
                var plexMovie = dbUpdateResult.Value[i];
                plexMovie.ShouldNotBeNull();
                plexMovie.Title.Contains("Updated!").ShouldBeTrue();
                plexMovie.Key.ShouldBeGreaterThan(0);
                plexMovie.Id.ShouldBeGreaterThan(0);
            }
        }

        [Fact]
        public async Task CreateUpdateOrDeletePlexMoviesCommand_DeleteMovies()
        {
            // Arrange
            SetupDatabase();
            var plexLibrary = FakeData.GetPlexLibrary(1, 1, PlexMediaType.Movie, _numberOfMovies).Generate();
            var plexMovies = FakeData.GetPlexMovies(_numberOfMovies);
            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));
            createResult.IsSuccess.ShouldBeTrue(createResult.Errors.ToString());

            var dbResult = await _mediator.Send(new GetPlexMoviesByPlexLibraryId(plexLibrary.Id));
            dbResult.IsSuccess.ShouldBeTrue();

            // remove the plexMovies
            dbResult.Value.RemoveRange(_numberOfMoviesHalf, _numberOfMoviesHalf);
            var updatePlexMovies = dbResult.Value;

            // Act
            plexLibrary.Movies = updatePlexMovies;
            var updatedResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));

            var dbUpdateResult = await _mediator.Send(new GetPlexMoviesByPlexLibraryId(plexLibrary.Id));

            // Assert
            createResult.Value.ShouldBeTrue();
            updatedResult.Value.ShouldBeTrue();
            updatedResult.IsSuccess.ShouldBeTrue();
            dbUpdateResult.IsSuccess.ShouldBeTrue();

            dbUpdateResult.Value.Count.ShouldBe(_numberOfMoviesHalf);


        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}