﻿using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.PlexMovies
{
    public class GetMultiplePlexMoviesByIdsQueryHandlerValidator : AbstractValidator<GetMultiplePlexMoviesByIdsQuery>
    {
        public GetMultiplePlexMoviesByIdsQueryHandlerValidator()
        {
            RuleFor(x => x.Ids.Count).GreaterThan(0);
        }
    }

    public class GetMultiplePlexMoviesByIdsQueryHandlerHandler : BaseHandler, IRequestHandler<GetMultiplePlexMoviesByIdsQuery, Result<List<PlexMovie>>>
    {
        public GetMultiplePlexMoviesByIdsQueryHandlerHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<PlexMovie>>> Handle(GetMultiplePlexMoviesByIdsQuery request, CancellationToken cancellationToken)
        {
            var query = PlexMoviesQueryable;

            if (request.IncludeLibrary)
            {
                query = query.IncludePlexLibrary();
            }

            if ( request.IncludeServer)
            {
                query = query.IncludePlexServer();
            }

            var plexMovies = await query.Where(x => request.Ids.Contains(x.Id)).ToListAsync(cancellationToken);

            return Result.Ok(plexMovies);
        }
    }
}