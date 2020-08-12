using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexServers.Queries
{
    public class GetPlexServerByIdWithLibrariesQuery : IRequest<Result<PlexServer>>
    {
        public GetPlexServerByIdWithLibrariesQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetPlexServerByIdWithLibrariesQueryValidator : AbstractValidator<GetPlexServerByIdWithLibrariesQuery>
    {
        public GetPlexServerByIdWithLibrariesQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexServerByIdWithLibrariesQueryHandler : BaseHandler,
        IRequestHandler<GetPlexServerByIdWithLibrariesQuery, Result<PlexServer>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexServerByIdWithLibrariesQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexServer>> Handle(GetPlexServerByIdWithLibrariesQuery request,
            CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<GetPlexServerByIdWithLibrariesQuery, GetPlexServerByIdWithLibrariesQueryValidator>(request);
            if (result.IsFailed) return result;

            var plexServer = await _dbContext.PlexServers.Include(x => x.PlexLibraries)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);


            return ReturnResult(plexServer, request.Id);

        }
    }
}
