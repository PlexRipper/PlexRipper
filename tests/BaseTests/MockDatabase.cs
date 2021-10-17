using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus.Extensions;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data;
using PlexRipper.Domain;
using Xunit.Abstractions;

namespace PlexRipper.BaseTests
{
    public static class MockDatabase
    {
        public static PlexRipperDbContext GetMemoryDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlexRipperDbContext>();
            optionsBuilder.UseInMemoryDatabase("memory_database");
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
    }
}