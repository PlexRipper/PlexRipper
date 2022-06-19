#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using JetBrains.Annotations;
using Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Data;
using PlexRipper.Domain;
using Shouldly;

#endregion

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
        /// <returns>A <see cref="PlexRipperDbContext" /> in memory instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static PlexRipperDbContext GetMemoryDbContext(string dbName = "", bool disableForeignKeyCheck = false)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlexRipperDbContext>();
            dbName = string.IsNullOrEmpty(dbName) ? GetMemoryDatabaseName() : dbName;

            // https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/in-memory-databases
            var connectionString = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.Memory,
                ForeignKeys = !disableForeignKeyCheck,

                // Database name
                DataSource = dbName,
                Cache = SqliteCacheMode.Shared,
            }.ToString();

            optionsBuilder.UseSqlite(connectionString);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.LogTo(text => Log.DbContextLogger(text), LogLevel.Error);
            return new PlexRipperDbContext(optionsBuilder.Options, dbName);
        }

        public static Task<PlexRipperDbContext> Setup(this PlexRipperDbContext context, int seed)
        {
            return context.Setup(config => { config.Seed = seed; });
        }

        public static async Task<PlexRipperDbContext> Setup(this PlexRipperDbContext context, [CanBeNull] Action<UnitTestDataConfig> options = null)
        {
            var config = new UnitTestDataConfig();
            options?.Invoke(config);

            // PlexServers and Libraries added
            Log.Debug($"Setting up {nameof(PlexRipperDbContext)} for {config.MemoryDbName}");

            context = await context.AddPlexServers(config);
            context = await context.AddPlexLibraries(config);
            context = await context.AddPlexAccount(config);

            if (config.MovieCount > 0)
            {
                context = await context.AddPlexMovies(config);
            }

            if (config.TvShowCount > 0)
            {
                context = await context.AddPlexTvShows(config);
            }

            if (config.MovieDownloadTasksCount > 0)
            {
                context = await context.AddMovieDownloadTasks(config);
            }

            if (config.TvShowDownloadTasksCount > 0)
            {
                context = await context.AddTvShowDownloadTasks(config);
            }

            return context;
        }

        public static async Task<PlexRipperDbContext> AddPlexServers(this PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var plexServers = FakeData.GetPlexServer(config).Generate(config.PlexServerCount);

            await dbContext.PlexServers.AddRangeAsync(plexServers);

            await dbContext.SaveChangesAsync();

            Log.Debug($"Added {config.PlexServerCount} {nameof(PlexServer)}s to {nameof(PlexRipperDbContext)}: {config.MemoryDbName}");
            return dbContext;
        }

        private static async Task<PlexRipperDbContext> AddPlexLibraries(this PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            var plexServers = await dbContext.PlexServers.ToListAsync();
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
                    {
                        plexLibrary.Type = PlexMediaType.Movie;
                    }
                }

                plexLibrariesToDb.AddRange(plexLibraries);
            }

            await dbContext.BulkInsertAsync(plexLibrariesToDb);
            return dbContext;
        }

        public static async Task<PlexRipperDbContext> AddPlexAccount(this PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();
            var plexServers = dbContext.PlexServers.Include(x => x.PlexLibraries).ToList();

            var plexAccount = FakeData.GetPlexAccount(config).Generate();

            await dbContext.PlexAccounts.AddAsync(plexAccount);
            await dbContext.SaveChangesAsync();
            Log.Debug($"Added 1 {nameof(PlexAccount)}: {plexAccount.Title} to {nameof(PlexRipperDbContext)}: {config.MemoryDbName}");

            var plexAccountServer = plexServers.Select(x => new PlexAccountServer
            {
                AuthTokenCreationDate = DateTime.Now,
                PlexServerId = x.Id,
                PlexAccountId = plexAccount.Id,
                AuthToken = "FAKE_AUTH_TOKEN",
                Owned = true,
            });

            // Add account -> server relation
            dbContext.PlexAccountServers.AddRange(plexAccountServer);
            await dbContext.SaveChangesAsync();

            // Add account -> library relation
            var plexAccountLibraries = plexServers.SelectMany(x => x.PlexLibraries).Select(x => new PlexAccountLibrary
            {
                PlexAccountId = plexAccount.Id,
                PlexServerId = x.PlexServerId,
                PlexLibraryId = x.Id,
            });
            dbContext.PlexAccountLibraries.AddRange(plexAccountLibraries);
            await dbContext.SaveChangesAsync();

            return dbContext;
        }

        #region Add DownloadTasks

        public static async Task<PlexRipperDbContext> AddMovieDownloadTasks(this PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var downloadTasks = FakeData.GetMovieDownloadTask(config).Generate(config.MovieDownloadTasksCount);
            var plexLibrary = dbContext.PlexLibraries.FirstOrDefault(x => x.Type == PlexMediaType.Movie);
            plexLibrary.ShouldNotBeNull();

            downloadTasks = downloadTasks.SetIds(plexLibrary.PlexServerId, plexLibrary.Id);

            dbContext.DownloadTasks.AddRange(downloadTasks);
            await dbContext.SaveChangesAsync();

            return dbContext;
        }

        public static async Task<PlexRipperDbContext> AddTvShowDownloadTasks(this PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var downloadTasks = FakeData.GetTvShowDownloadTask(config).Generate(config.TvShowDownloadTasksCount);
            var plexLibrary = dbContext.PlexLibraries.FirstOrDefault(x => x.Type == PlexMediaType.TvShow);
            plexLibrary.ShouldNotBeNull();

            downloadTasks = downloadTasks.SetIds(plexLibrary.PlexServerId, plexLibrary.Id);

            dbContext.DownloadTasks.AddRange(downloadTasks);
            await dbContext.SaveChangesAsync();

            return dbContext;
        }

        #endregion

        #region Add Media

        private static async Task<PlexRipperDbContext> AddPlexMovies(this PlexRipperDbContext context, UnitTestDataConfig config = null)
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

            await context.SaveChangesAsync();

            return context;
        }

        private static async Task<PlexRipperDbContext> AddPlexTvShows(this PlexRipperDbContext context, UnitTestDataConfig config = null)
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

            await context.SaveChangesAsync();

            return context;
        }

        #endregion
    }
}