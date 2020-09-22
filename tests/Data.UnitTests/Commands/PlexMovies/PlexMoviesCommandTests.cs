using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using PlexRipper.Application.PlexMovies;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Fixtures;
using PlexRipper.Data;
using PlexRipper.Data.Commands.PlexMovies;
using PlexRipper.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Data.UnitTests
{
    [Collection("PlexMoviesCommandTests")]
    public class PlexMoviesCommandTests : IDisposable
    {
        private BaseContainer Container { get; }

        private PlexRipperDbContext _dbContext { get; }

        private IMediator _mediator { get; }

        private const int _numberOfMovies = 100;

        public PlexMoviesCommandTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();
            _dbContext = Container.PlexRipperDbContext;
            _mediator = Container.Mediator;
        }

        private void SetupDatabase()
        {
            _dbContext.PlexServers.Add(new PlexServer
            {
                Id = 1,
                Name = "TestPlexServer",
            });
            _dbContext.PlexLibraries.Add(new PlexLibrary
            {
                Id = 1,
                Title = "TestMovieLibrary",
                Type = "movie",
                PlexServerId = 1,
            });
            _dbContext.SaveChanges();
        }

        private List<PlexMovie> GetFakePlexMovies()
        {
            var plexMovies = new List<PlexMovie>();
            for (int i = 1; i <= _numberOfMovies; i++)
            {
                plexMovies.Add(new PlexMovie
                {
                    RatingKey = i,
                    Title = $"Fake movie {i}",
                    PlexLibraryId = 1,
                    PlexMovieDatas = new List<PlexMovieData>
                    {
                        new PlexMovieData
                        {
                            MediaFormat = "ADDED",
                            Height = i * 100,
                            Width = i * 100,
                            Parts = new List<PlexMovieDataPart>
                            {
                                new PlexMovieDataPart
                                {
                                    Container = "mkv",
                                    File = "ADDED",
                                    Key = (i * 5).ToString(),
                                },
                            },
                        },
                    },
                });
            }

            return plexMovies;
        }

        [Fact]
        public async Task CreateOrUpdatePlexMoviesCommand_CreateMovies()
        {
            // Arrange
            SetupDatabase();

            var command = new CreateUpdateOrDeletePlexMoviesCommand(new PlexLibrary
            {
                Id = 1,
                Title = "TestMovieLibrary",
                UpdatedAt = DateTime.MinValue,
            }, GetFakePlexMovies());

            // Act
            var result = await _mediator.Send(command);
            var dbResult = _dbContext.PlexMovies.Where(x => x.PlexLibraryId == 1).ToList();

            // Assert
            result.IsFailed.Should().BeFalse();
            result.Value.Should().BeTrue();

            dbResult.Count.Should().Be(_numberOfMovies);
            foreach (var plexMovie in dbResult)
            {
                plexMovie.Should().NotBeNull();
                plexMovie.Title.Should().NotBeEmpty();
                plexMovie.RatingKey.Should().BeGreaterThan(0);
                plexMovie.Id.Should().BeGreaterThan(0);
            }
        }

        [Fact]
        public async Task CreateOrUpdatePlexMoviesCommand_UpdateMovies()
        {
            // Arrange
            SetupDatabase();
            var plexLibrary = new PlexLibrary
            {
                Id = 1,
                Title = "TestMovieLibrary",
            };
            var plexMovies = GetFakePlexMovies();
            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary, plexMovies));
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
                                Key = x.Id.ToString(),
                            },
                        },
                    });
                }

                return x;
            }).ToList();

            // Act
            var updatedResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary, updatePlexMovies));
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

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}