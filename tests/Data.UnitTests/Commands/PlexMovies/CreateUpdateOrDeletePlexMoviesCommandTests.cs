using System;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using PlexRipper.Application.PlexMovies;
using PlexRipper.BaseTests;
using PlexRipper.Data;
using PlexRipper.Domain;
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
            createResult.IsFailed.Should().BeFalse();
            createResult.Value.Should().BeTrue();

            dbResult.Value.Count.Should().Be(_numberOfMovies);
            foreach (var plexMovie in dbResult.Value)
            {
                plexMovie.Should().NotBeNull();
                plexMovie.Title.Should().NotBeEmpty();
                plexMovie.Key.Should().BeGreaterThan(0);
                plexMovie.Id.Should().BeGreaterThan(0);
            }
        }

        [Fact]
        public async Task CreateUpdateOrDeletePlexMoviesCommand_UpdateMovies()
        {
            // Arrange
            SetupDatabase();
            var plexLibrary = FakeData.GetPlexLibrary(1, 1, PlexMediaType.Movie, _numberOfMovies).Generate();
            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));
            createResult.IsSuccess.Should().BeTrue(createResult.Errors.ToString());

            var dbResult = await _mediator.Send(new GetPlexMoviesByPlexLibraryId(plexLibrary.Id));
            dbResult.IsSuccess.Should().BeTrue();

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
            updatedResult.IsSuccess.Should().BeTrue();

            var dbUpdateResult = await _mediator.Send(new GetPlexMoviesByPlexLibraryId(plexLibrary.Id));
            dbUpdateResult.IsSuccess.Should().BeTrue();

            // Assert
            createResult.Value.Should().BeTrue();
            updatedResult.IsFailed.Should().BeFalse();
            updatedResult.Value.Should().BeTrue();

            dbUpdateResult.Value.Count.Should().Be(_numberOfMovies);
            for (int i = 0; i < 50; i++)
            {
                var plexMovie = dbUpdateResult.Value[i];
                plexMovie.Should().NotBeNull();
                plexMovie.Title.Contains("Updated!").Should().BeTrue();
                plexMovie.Key.Should().BeGreaterThan(0);
                plexMovie.Id.Should().BeGreaterThan(0);
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
            createResult.IsSuccess.Should().BeTrue(createResult.Errors.ToString());

            var dbResult = await _mediator.Send(new GetPlexMoviesByPlexLibraryId(plexLibrary.Id));
            dbResult.IsSuccess.Should().BeTrue();

            // remove the plexMovies
            dbResult.Value.RemoveRange(_numberOfMoviesHalf, _numberOfMoviesHalf);
            var updatePlexMovies = dbResult.Value;

            // Act
            plexLibrary.Movies = updatePlexMovies;
            var updatedResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));

            var dbUpdateResult = await _mediator.Send(new GetPlexMoviesByPlexLibraryId(plexLibrary.Id));

            // Assert
            createResult.Value.Should().BeTrue();
            updatedResult.Value.Should().BeTrue();
            updatedResult.IsSuccess.Should().BeTrue();
            dbUpdateResult.IsSuccess.Should().BeTrue();

            dbUpdateResult.Value.Count.Should().Be(_numberOfMoviesHalf);


        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}