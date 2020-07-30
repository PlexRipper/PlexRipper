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
    public class GetPlexServerByIdQuery : IRequest<Result<PlexServer>>
    {
        public GetPlexServerByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetPlexServerByIdQueryQueryValidator : AbstractValidator<GetPlexServerByIdQuery>
    {
        public GetPlexServerByIdQueryQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexServerByIdQueryHandler : BaseHandler,
        IRequestHandler<GetPlexServerByIdQuery, Result<PlexServer>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexServerByIdQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexServer>> Handle(GetPlexServerByIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<GetPlexServerByIdQuery, GetPlexServerByIdQueryQueryValidator>(request);
            if (result.IsFailed) return result;

            var plexServer = await _dbContext.PlexServers.Include(x => x.PlexLibraries)
                .FirstOrDefaultAsync(x => x.Id == request.Id);


            return ReturnResult(plexServer, request.Id);

        }
    }
}
