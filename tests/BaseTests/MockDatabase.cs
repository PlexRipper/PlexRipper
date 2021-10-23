using System;
using System.Collections.Generic;
using System.Linq;
using Bogus.Extensions;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public static class MockDatabase
    {
        private static Random _rnd = new Random();

        public static PlexRipperDbContext GetMemoryDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlexRipperDbContext>();

            optionsBuilder.UseInMemoryDatabase($"memory_database_{_rnd.Next(1, int.MaxValue)}_{_rnd.Next(1, int.MaxValue)}");
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new PlexRipperDbContext(optionsBuilder.Options);
        }

        public static PlexRipperDbContext AddPlexServers(this PlexRipperDbContext dbContext, int serverCount = 1)
        {
            var plexServers = FakeData.GetPlexServer(new() { IncludeLibraries = true }).Generate(serverCount);
            foreach (var plexServer in plexServers)
            {
                plexServer.Id = 0;
                foreach (var plexLibrary in plexServer.PlexLibraries)
                {
                    plexLibrary.Id = 0;
                }
            }

            dbContext.PlexServers.AddRange(plexServers);

            dbContext.SaveChanges();
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

            dbContext.DownloadTasks.AddRange(downloadTasks);

            dbContext.SaveChanges();
            return dbContext;
        }
    }
}