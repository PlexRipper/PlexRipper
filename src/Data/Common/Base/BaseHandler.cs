﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Data.Common
{
    public abstract class BaseHandler
    {
        #region Fields

        private protected readonly PlexRipperDbContext _dbContext;

        private protected readonly BulkConfig _bulkConfig = new BulkConfig
        {
            SetOutputIdentity = true,
            PreserveInsertOrder = true,
        };

        #endregion

        #region Constructors

        protected BaseHandler(PlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        #region Properties

        protected IQueryable<PlexLibrary> PlexLibraryQueryable => _dbContext.PlexLibraries.AsQueryable();

        /// <summary>
        /// Creates a PlexMovie IQueryable with all PlexMovie data included -> PlexMovieDatas -> Parts.
        /// </summary>
        protected IQueryable<PlexMovie> PlexMoviesQueryable => _dbContext.PlexMovies.AsQueryable().IncludeMovieData();

        #endregion

        #region Methods

        /// <summary>
        /// Only include <see cref="PlexLibrary"/> related data based on the <see cref="PlexMediaType"/>.
        /// </summary>
        /// <param name="type">The <see cref="PlexMediaType"/> to use to include the related data.</param>
        /// <param name="includeServer">Optionally include the <see cref="PlexServer"/> this <see cref="PlexLibrary"/> belongs to.</param>
        /// <param name="includeMedia"></param>
        /// <param name="topLevelMediaOnly"></param>
        /// <returns>The <see cref="IQueryable"/> of <see cref="PlexLibrary"/>.</returns>
        protected IQueryable<PlexLibrary> GetPlexLibraryQueryableByType(PlexMediaType type, bool includeServer = false, bool includeMedia = false,
            bool topLevelMediaOnly = false)
        {
            var plexLibraryQuery = PlexLibraryQueryable;

            if (includeServer)
            {
                plexLibraryQuery = plexLibraryQuery.IncludeServer();
            }

            if (includeMedia)
            {
                switch (type)
                {
                    case PlexMediaType.Movie:
                        return plexLibraryQuery.IncludeMovies(topLevelMediaOnly);
                    case PlexMediaType.TvShow:
                        return plexLibraryQuery.IncludeTvShows(topLevelMediaOnly);
                    default:
                        Log.Error($"PlexLibrary with MediaType {type} is currently not supported");
                        return plexLibraryQuery;
                }
            }

            return plexLibraryQuery;
        }

        protected Result<T> ReturnResult<T>(T value, int id = 0)
            where T : BaseEntity
        {
            if (value != null) return Result.Ok(value);

            return Result.Fail(new Error($"Could not find an entity of {typeof(T)} with an id of {id}"));
        }

        protected Result<List<T>> ReturnResult<T>(List<T> value)
        {
            if (value != null && value.Any()) return Result.Ok(value);

            return Result.Fail(new Error($"Could not find entities of {typeof(T)}"));
        }

        protected async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        #endregion
    }
}