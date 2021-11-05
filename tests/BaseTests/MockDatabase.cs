using System;
using System.Collections.Generic;
using System.Linq;
using Bogus.Extensions;
using EFCore.BulkExtensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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

        public static PlexRipperDbContext GetMemoryDbContext(string dbName = "")
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlexRipperDbContext>();

            // https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/in-memory-databases
            var connectionString = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.Memory,

                // Database name
                DataSource = string.IsNullOrEmpty(dbName) ? GetMemoryDatabaseName() : dbName,
                Cache = SqliteCacheMode.Shared,
            }.ToString();

            optionsBuilder.UseSqlite(connectionString);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.EnableSensitiveDataLogging();
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
            dbContext.BulkInsert(plexServers.SelectMany(x => x.PlexLibraries).ToList());

            return dbContext;
        }

        public static PlexRipperDbContext AddMedia(this PlexRipperDbContext dbContext)
        {
            var plexLibraries = dbContext.PlexLibraries.ToList();

            if (plexLibraries.Any())
            {
                foreach (var plexLibrary in plexLibraries)
                {
                    switch (plexLibrary.Type)
                    {
                        case PlexMediaType.Movie:
                            dbContext.PlexMovies.AddRange(FakeData.GetPlexMovies().GenerateBetween(20, 100));
                            break;
                        case PlexMediaType.TvShow:
                            dbContext.PlexTvShows.AddRange(FakeData.GetPlexTvShows().GenerateBetween(10, 20));
                            break;
                        default:
                            throw new NotSupportedException($"{plexLibrary.Type} not supported in MockDatabase.AddMedia");
                    }
                }

                dbContext.SaveChanges();
            }

            return dbContext;
        }

        public static PlexRipperDbContext AddDownloadTasks(this PlexRipperDbContext dbContext, FakeDataConfig config = null)
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

            foreach (var downloadTask in downloadTasks.Flatten(x => x.Children).ToList())
            {
                downloadTask.PlexServer = null;
                downloadTask.PlexLibrary = null;
                downloadTask.DestinationFolder = null;
                downloadTask.DownloadFolder = null;
            }

            dbContext.DownloadTasks.AddRange(downloadTasks);

            dbContext.SaveChanges();
            return dbContext;
        }
    }
}