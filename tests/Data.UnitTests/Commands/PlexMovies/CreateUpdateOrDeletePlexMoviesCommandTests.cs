using System;
using System.Collections.Generic;
using System.Linq;
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
        private int _numberOfMoviesHalf = (int)Math.Floor(_numberOfMovies / (double)2);

        public CreateUpdateOrDeletePlexMoviesCommandTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();
            _dbContext = Container.PlexRipperDbContext;
            _mediator = Container.Mediator;
        }

        private void SetupDatabase()
        {
            _dbContext.PlexServers.Add(FakeDbData.GetPlexServer());
            _dbContext.PlexLibraries.Add(FakeDbData.GetPlexLibrary(1, 1, PlexMediaType.TvShow).Generate());
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task CreateUpdateOrDeletePlexMoviesCommand_CreateMovies()
        {
            // Arrange
            SetupDatabase();
            var plexLibrary = FakeDbData.GetPlexLibrary(1, 1, PlexMediaType.Movie).Generate();

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
                plexMovie.RatingKey.Should().BeGreaterThan(0);
                plexMovie.Id.Should().BeGreaterThan(0);
            }
        }

        [Fact]
        public async Task CreateUpdateOrDeletePlexMoviesCommand_UpdateMovies()
        {
            // Arrange
            SetupDatabase();
            var plexLibrary = FakeDbData.GetPlexLibrary(1, 1, PlexMediaType.Movie).Generate();
            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));
            createResult.IsSuccess.Should().BeTrue(createResult.Errors.ToString());

            var dbResult = await _mediator.Send(new GetPlexMoviesByPlexLibraryId(plexLibrary.Id));
            dbResult.IsSuccess.Should().BeTrue();

            // change the plexMovies
            var updatePlexMovies = dbResult.Value.Select(x =>
            {
                if (x.Id % 2 == 0)
                {
                    x.Title += " Updated!";
                    x.UpdatedAt = x.UpdatedAt.AddDays(4);
                    x.PlexMovieDatas.Add(new PlexMovieData
                    {
                        MediaFormat = "Updated!",
                        Height = x.Id * 100,
                        Width = x.Id * 100,
                        Parts = new List<PlexMovieDataPart>
                        {
                            new PlexMovieDataPart
                            {
                                Container = "mkv",
                                File = "Updated!",
                                ObfuscatedFilePath = x.Id.ToString(),
                            },
                        },
                    });
                }

                return x;
            }).ToList();

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
            foreach (var plexMovie in dbUpdateResult.Value)
            {
                plexMovie.Should().NotBeNull();
                plexMovie.Title.Should().NotBeEmpty();
                plexMovie.RatingKey.Should().BeGreaterThan(0);
                plexMovie.Id.Should().BeGreaterThan(0);
                if (plexMovie.Id % 2 == 0)
                {
                    plexMovie.Title.Contains("Updated!").Should().BeTrue();
                }
            }
        }

        [Fact]
        public async Task CreateUpdateOrDeletePlexMoviesCommand_DeleteMovies()
        {
            // Arrange
            SetupDatabase();
            var plexLibrary = FakeDbData.GetPlexLibrary(1, 1, PlexMediaType.Movie).Generate();
            var plexMovies = FakeDbData.GetPlexMovies(_numberOfMovies);
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
            updatedResult.IsSuccess.Should().BeTrue();

            var dbUpdateResult = await _mediator.Send(new GetPlexMoviesByPlexLibraryId(plexLibrary.Id));
            dbUpdateResult.IsSuccess.Should().BeTrue();

            // Assert
            createResult.Value.Should().BeTrue();
            updatedResult.IsFailed.Should().BeFalse();
            updatedResult.Value.Should().BeTrue();

            dbUpdateResult.Value.Count.Should().Be(_numberOfMoviesHalf);
            foreach (var plexMovie in dbUpdateResult.Value)
            {
                plexMovie.Should().NotBeNull();
                plexMovie.Title.Should().NotBeEmpty();
                plexMovie.RatingKey.Should().BeGreaterThan(0);
                plexMovie.Id.Should().BeGreaterThan(0);
            }
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}