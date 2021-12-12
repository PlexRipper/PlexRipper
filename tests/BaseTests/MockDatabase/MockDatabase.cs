using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.BulkExtensions;
using Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlexRipper.Data;
using PlexRipper.Domain;
using Shouldly;

namespace PlexRipper.BaseTests
{
    public static class MockDatabase
    {
        private static readonly Random Rnd = new();

        public static string GetMemoryDatabaseName()
        {
            return $"memory_database_{Rnd.Next(1, int.MaxValue)}_{Rnd.Next(1, int.MaxValue)}";
        }

        /// <summary>
        /// Creates an in-memory database only to be used for unit and integration testing.
        /// </summary>
        /// <param name="dbName">leave empty to generate a random one</param>
        /// <param name="disableForeignKeyCheck">By default, don't enforce foreign key check for handling database data.</param>
        /// <returns>A <see cref="PlexRipperDbContext"/> in memory instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static PlexRipperDbContext GetMemoryDbContext(string dbName = "", bool disableForeignKeyCheck = false)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlexRipperDbContext>();

            // https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/in-memory-databases
            var connectionString = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.Memory,
                ForeignKeys = !disableForeignKeyCheck,

                // Database name
                DataSource = string.IsNullOrEmpty(dbName) ? GetMemoryDatabaseName() : dbName,
                Cache = SqliteCacheMode.Shared,
            }.ToString();

            optionsBuilder.UseSqlite(connectionString);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.LogTo(text => Log.DbContextLogger(text), LogLevel.Error);
            return new PlexRipperDbContext(optionsBuilder.Options);
        }

        public static PlexRipperDbContext Setup(this PlexRipperDbContext context, UnitTestDataConfig config)
        {
            // PlexServers and Libraries added
            context = context.AddPlexServers(config).AddPlexLibraries(config);

            if (config.MovieCount > 0)
            {
                context = context.AddPlexMovies(config);
            }

            if (config.TvShowCount > 0)
            {
                context = context.AddPlexTvShows(config);
            }

            return context;
        }

        public static PlexRipperDbContext AddPlexServers(this PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var plexServers = FakeData.GetPlexServer(config).Generate(config.PlexServerCount);

            dbContext.PlexServers.AddRange(plexServers);

            dbContext.SaveChanges();

            return dbContext;
        }

        private static PlexRipperDbContext AddPlexLibraries(this PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            var plexServers = dbContext.PlexServers.ToList();
            plexServers.ShouldNotBeEmpty();

            config ??= new UnitTestDataConfig();
            config.PlexLibraryCount.ShouldBeGreaterThanOrEqualTo(2);
            var plexLibrariesToDb = new List<PlexLibrary>();

            foreach (var plexServer in plexServers)
            {
                var plexLibraries = FakeData.GetPlexLibrary().Generate(config.PlexLibraryCount);
                plexLibraries[0].Type = PlexMediaType.Movie;
                plexLibraries[1].Type = PlexMediaType.TvShow;

                foreach (var plexLibrary in plexLibraries)
                {
                    plexLibrary.PlexServerId = plexServer.Id;
                    if (plexLibrary.Type == PlexMediaType.None)
                        plexLibrary.Type = PlexMediaType.Movie;
                }

                plexLibrariesToDb.AddRange(plexLibraries);
            }

            dbContext.BulkInsert(plexLibrariesToDb);
            return dbContext;
        }

        public static PlexRipperDbContext AddDownloadTasks(this PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            try
            {
                List<DownloadTask> downloadTasks;
                switch (config.LibraryType)
                {
                    case PlexMediaType.Movie:
                        downloadTasks = FakeData.GetMovieDownloadTask(config).Generate(config.DownloadTasksCount);
                        break;
                    case PlexMediaType.TvShow:
                        downloadTasks = FakeData.GetTvShowDownloadTask(config).Generate(config.DownloadTasksCount);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var plexServer = dbContext.PlexServers.Include(x => x.PlexLibraries).FirstOrDefault();
                var plexLibrary = plexServer.PlexLibraries.FirstOrDefault(x => x.Type == config.LibraryType);
                if (plexServer is null || plexLibrary is null)
                {
                    throw new ArgumentNullException($"Ensure {nameof(AddPlexServers)} has been called before {nameof(AddDownloadTasks)}");
                }

                foreach (var downloadTask in downloadTasks.Flatten(x => x.Children).ToList())
                {
                    downloadTask.PlexServerId = plexServer?.Id ?? 1;
                    downloadTask.PlexLibraryId = plexLibrary?.Id ?? 1;
                    downloadTask.PlexServer = null;
                    downloadTask.PlexLibrary = null;
                    downloadTask.DestinationFolder = null;
                    downloadTask.DownloadFolder = null;
                    downloadTask.Parent = null;
                }

                dbContext.DownloadTasks.AddRange(downloadTasks);

                dbContext.SaveChanges();
                return dbContext;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static PlexRipperDbContext AddPlexMovies(this PlexRipperDbContext context, UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig
            {
                IncludeLibraries = true,
            };

            var plexLibraries = context.PlexLibraries.Where(x => x.Type == PlexMediaType.Movie).ToList();
            plexLibraries.ShouldNotBeNull().ShouldNotBeEmpty();

            foreach (var plexLibrary in plexLibraries)
            {
                var movies = FakeData.GetPlexMovies(config).Generate(config.MovieCount);

                foreach (var movie in movies)
                {
                    movie.PlexLibraryId = plexLibrary.Id;
                    movie.PlexServerId = plexLibrary.PlexServerId;
                }

                context.PlexMovies.AddRange(movies);
            }

            context.SaveChanges();

            return context;
        }

        private static PlexRipperDbContext AddPlexTvShows(this PlexRipperDbContext context, UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig
            {
                IncludeLibraries = true,
            };

            var plexLibraries = context.PlexLibraries.Where(x => x.Type == PlexMediaType.TvShow).ToList();
            plexLibraries.ShouldNotBeNull().ShouldNotBeEmpty();

            foreach (var plexLibrary in plexLibraries)
            {
                var tvShows = FakeData.GetPlexTvShows(config).Generate(config.TvShowCount);

                foreach (var tvShow in tvShows)
                {
                    tvShow.PlexLibraryId = plexLibrary.Id;
                    tvShow.PlexServerId = plexLibrary.PlexServerId;

                    foreach (var season in tvShow.Seasons)
                    {
                        season.TvShow = tvShow;
                        season.PlexLibraryId = plexLibrary.Id;
                        season.PlexServerId = plexLibrary.PlexServerId;

                        foreach (var episode in season.Episodes)
                        {
                            episode.TvShow = tvShow;
                            episode.TvShowSeason = season;
                            episode.PlexLibraryId = plexLibrary.Id;
                            episode.PlexServerId = plexLibrary.PlexServerId;
                        }
                    }
                }

                context.PlexTvShows.AddRange(tvShows);
            }

            context.SaveChanges();

            return context;
        }
    }
}