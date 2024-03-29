﻿using Application.Contracts;
using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Validators;

namespace PlexRipper.Application.UnitTests;

public class GenerateDownloadTaskMoviesCommandHandler_UnitTests : BaseUnitTest<GenerateDownloadTaskMoviesCommandHandler>
{
    private DownloadTaskMovieValidator validator = new();

    public GenerateDownloadTaskMoviesCommandHandler_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveInsertedValidDownloadTaskMoviesInDatabase_WhenGivenValidPlexMovies()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 5;
        });

        var plexMovies = await IDbContext.PlexMovies.ToListAsync();
        var movies = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Movie,
                MediaIds = plexMovies.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };

        // Act
        var command = new GenerateDownloadTaskMoviesCommand(movies);
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        ResetDbContext();
        var plexDownloadTaskMovies = await IDbContext
            .DownloadTaskMovie
            .IncludeAll()
            .ToListAsync();

        plexDownloadTaskMovies.Count.ShouldBe(5);

        foreach (var downloadTaskMovie in plexDownloadTaskMovies)
        {
            downloadTaskMovie.Calculate();
            (await validator.ValidateAsync(downloadTaskMovie)).Errors.ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task ShouldHaveMultipleDownloadTaskMovieFile_WhenPlexMovieHasMultiParts()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 2;
            config.IncludeMultiPartMovies = true;
        });

        var plexMovies = await IDbContext.PlexMovies.ToListAsync();
        var movies = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Movie,
                MediaIds = plexMovies.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };

        // Act
        var command = new GenerateDownloadTaskMoviesCommand(movies);
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        ResetDbContext();
        var plexDownloadTaskMovies = await IDbContext
            .DownloadTaskMovie
            .IncludeAll()
            .ToListAsync();

        plexDownloadTaskMovies.Count.ShouldBe(2);

        foreach (var downloadTaskMovie in plexDownloadTaskMovies)
        {
            downloadTaskMovie.Calculate();
            (await validator.ValidateAsync(downloadTaskMovie)).Errors.ShouldBeEmpty();
            downloadTaskMovie.Children.Count.ShouldBe(2);
        }
    }
}