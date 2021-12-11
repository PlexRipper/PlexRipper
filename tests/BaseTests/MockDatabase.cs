using System;
using System.Collections.Generic;
using System.Linq;
using Bogus.Extensions;
using EFCore.BulkExtensions;
using Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlexRipper.Data;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public static class MockDatabase
    {
        private static Random _rnd = new Random();

        public static string GetMemoryDatabaseName()
        {
            return $"memory_database_{_rnd.Next(1, int.MaxValue)}_{_rnd.Next(1, int.MaxValue)}";
        }

        /// <summary>
        /// Creates an in-memory database only to be used for unit and integration testing.
        /// </summary>
        /// <param name="dbName">leave empty to generate a random one</param>
        /// <param name="disableForeignKeyCheck">By default, don't enforce foreign key check for handling database data.</param>
        /// <returns>A <see cref="PlexRipperDbContext"/> in memory instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static PlexRipperDbContext GetMemoryDbContext(string dbName = "", bool disableForeignKeyCheck = true)
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
            optionsBuilder.LogTo((_, level) =>
            {
                switch (level)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                    case LogLevel.Information:
                        return false;
                    case LogLevel.Warning:
                    case LogLevel.Error:
                    case LogLevel.Critical:
                    case LogLevel.None:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }
            }, text => Log.Debug($"{text}"));
            return new PlexRipperDbContext(optionsBuilder.Options);
        }

        public static PlexRipperDbContext AddPlexServers(this PlexRipperDbContext dbContext, FakeDataConfig config = null, int serverCount = 1)
        {
            config ??= new FakeDataConfig
            {
                IncludeLibraries = true,
            };

            var plexServers = FakeData.GetPlexServer(config).Generate(serverCount);
            foreach (var plexServer in plexServers)
            {
                plexServer.Id = 0;
                foreach (var plexLibrary in plexServer.PlexLibraries)
                {
                    plexLibrary.Id = 0;
                }
            }

            dbContext.BulkInsert(plexServers);
            var plexLibraries = plexServers.SelectMany(x => x.PlexLibraries).ToList();
            dbContext.BulkInsert(plexLibraries);

            return dbContext;
        }

        public static PlexRipperDbContext AddMedia(this PlexRipperDbContext dbContext, FakeDataConfig config = null)
        {
            config ??= new FakeDataConfig
            {
                IncludeLibraries = true,
            };

            var plexLibraries = dbContext.PlexLibraries.ToList();
            if (plexLibraries.Any())
            {
                foreach (var plexLibrary in plexLibraries)
                {
                    switch (plexLibrary.Type)
                    {
                        case PlexMediaType.Movie:
                            dbContext.PlexMovies.AddRange(FakeData.GetPlexMovies(config).GenerateBetween(20, 100));
                            break;
                        case PlexMediaType.TvShow:
                            dbContext.PlexTvShows.AddRange(FakeData.GetPlexTvShows(config).GenerateBetween(10, 20));
                            break;
                        default:
                            throw new NotSupportedException($"{plexLibrary.Type} not supported in MockDatabase.AddMedia");
                    }
                }

                dbContext.SaveChanges();
                return dbContext;
            }

            throw new Exception($"{nameof(AddMedia)} can only be used after PlexLibraries have been added");
        }

        public static PlexRipperDbContext AddDownloadTasks(this PlexRipperDbContext dbContext, FakeDataConfig config = null)
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
    }
}