using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.BulkExtensions;
using Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlexRipper.BaseTests.Extensions;
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
            context = context.AddPlexServers(config).AddPlexLibraries(config).AddPlexAccount(config);

            if (config.MovieCount > 0)
            {
                context = context.AddPlexMovies(config);
            }

            if (config.TvShowCount > 0)
            {
                context = context.AddPlexTvShows(config);
            }

            if (config.MovieDownloadTasksCount > 0)
            {
                context = context.AddMovieDownloadTasks(config);
            }

            if (config.TvShowDownloadTasksCount > 0)
            {
                context = context.AddTvShowDownloadTasks(config);
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

        public static PlexRipperDbContext AddPlexAccount(this PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();
            var plexServers = dbContext.PlexServers.Include(x => x.PlexLibraries).ToList();

            var plexAccount = FakeData.GetPlexAccount(config).Generate();

            dbContext.PlexAccounts.Add(plexAccount);
            dbContext.SaveChanges();

            // Add account -> server relation
            dbContext.PlexAccountServers.AddRange(plexServers.Select(x => new PlexAccountServer
            {
                AuthTokenCreationDate = DateTime.Now,
                PlexServerId = x.Id,
                PlexAccountId = 1,
                AuthToken = "FAKE_AUTH_TOKEN",
                Owned = true,
            }));

            // Add account -> library relation
            dbContext.PlexAccountLibraries.AddRange(plexServers.SelectMany(x => x.PlexLibraries).Select(x => new PlexAccountLibrary
            {
                PlexAccountId = 1,
                PlexServerId = x.PlexServerId,
                PlexLibraryId = x.Id,
            }));
            dbContext.SaveChanges();

            return dbContext;
        }

        private static PlexRipperDbContext AddPlexLibraries(this PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            var plexServers = dbContext.PlexServers.ToList();
            plexServers.ShouldNotBeEmpty();

            config ??= new UnitTestDataConfig();
            var plexLibrariesToDb = new List<PlexLibrary>();

            foreach (var plexServer in plexServers)
            {
                List<PlexLibrary> plexLibraries;
                if (config.PlexLibraryCount == 0)
                {
                    plexLibraries = FakeData.GetPlexLibrary().Generate(2);
                    plexLibraries[0].Type = PlexMediaType.Movie;
                    plexLibraries[1].Type = PlexMediaType.TvShow;
                }
                else
                {
                    plexLibraries = FakeData.GetPlexLibrary().Generate(config.PlexLibraryCount);
                    plexLibraries[0].Type = PlexMediaType.Movie;
                    plexLibraries[1].Type = PlexMediaType.TvShow;
                }

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

        #region Add DownloadTasks

        public static PlexRipperDbContext AddMovieDownloadTasks(this PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var downloadTasks = FakeData.GetMovieDownloadTask(config).Generate(config.MovieDownloadTasksCount);
            var plexLibrary = dbContext.PlexLibraries.FirstOrDefault(x => x.Type == PlexMediaType.Movie);
            plexLibrary.ShouldNotBeNull();

            downloadTasks = downloadTasks.SetIds(plexLibrary.PlexServerId, plexLibrary.Id);

            dbContext.DownloadTasks.AddRange(downloadTasks);
            dbContext.SaveChanges();

            return dbContext;
        }

        public static PlexRipperDbContext AddTvShowDownloadTasks(this PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var downloadTasks = FakeData.GetTvShowDownloadTask(config).Generate(config.TvShowDownloadTasksCount);
            var plexLibrary = dbContext.PlexLibraries.FirstOrDefault(x => x.Type == PlexMediaType.TvShow);
            plexLibrary.ShouldNotBeNull();

            downloadTasks = downloadTasks.SetIds(plexLibrary.PlexServerId, plexLibrary.Id);

            dbContext.DownloadTasks.AddRange(downloadTasks);
            dbContext.SaveChanges();

            return dbContext;
        }

        #endregion

        #region Add Media

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

        #endregion
    }
}