﻿using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexLibraries;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexLibraries
{
    public class GetPlexLibraryByIdWithServerQueryValidator : AbstractValidator<GetPlexLibraryByIdWithServerQuery>
    {
        public GetPlexLibraryByIdWithServerQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetPlexLibraryByIdWithServerQueryHandler : BaseHandler, IRequestHandler<GetPlexLibraryByIdWithServerQuery, Result<PlexLibrary>>
    {
        public GetPlexLibraryByIdWithServerQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexLibrary>> Handle(GetPlexLibraryByIdWithServerQuery request, CancellationToken cancellationToken)
        {
            var plexLibrary = await PlexLibraryQueryable.IncludeServer().FirstOrDefaultAsync(x => x.Id == request.Id);

            return ReturnResult(plexLibrary, request.Id);
        }
    }
}