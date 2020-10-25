using System.Collections.Generic;
using System.Linq;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Data.Common
{
    public abstract class BaseHandler
    {
        #region Fields

        private protected readonly PlexRipperDbContext _dbContext;

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

        #endregion
    }
}