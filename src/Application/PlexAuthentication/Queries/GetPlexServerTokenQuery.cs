using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexAuthentication.Queries
{
    public class GetPlexServerTokenQuery : IRequest<Result<string>>
    {
        public int PlexAccountId { get; }
        public int PlexServerId { get; }


        public GetPlexServerTokenQuery(int plexAccountId, int plexServerId)
        {
            PlexAccountId = plexAccountId;
            PlexServerId = plexServerId;
        }
    }

    public class GetPlexServerTokenQueryValidator : AbstractValidator<GetPlexServerTokenQuery>
    {
        public GetPlexServerTokenQueryValidator()
        {
            RuleFor(x => x.PlexAccountId).GreaterThan(0);
            RuleFor(x => x.PlexServerId).GreaterThan(0);
        }
    }


    public class GetPlexServerTokenHandler : IRequestHandler<GetPlexServerTokenQuery, Result<string>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexServerTokenHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<string>> Handle(GetPlexServerTokenQuery request, CancellationToken cancellationToken)
        {
            var authToken = await _dbContext.PlexAccountServers.FirstOrDefaultAsync(x => x.PlexAccountId == request.PlexAccountId && x.PlexServerId == request.PlexServerId);

            if (authToken != null)
            {
                return Result.Ok(authToken.AuthToken);
            }

            return Result.Fail(new Error($"Could not find an authenticationToken for PlexAccount with id: {request.PlexAccountId} and PlexServer with id: {request.PlexServerId}"));

        }
    }
}
