using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Base;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.PlexServers.Queries
{
    public class GetPlexServerStatusByIdQuery : IRequest<Result<PlexServerStatus>>
    {
        public GetPlexServerStatusByIdQuery(int id, bool includePlexServer = false)
        {
            Id = id;
            IncludePlexServer = includePlexServer;
        }

        public int Id { get; }
        public bool IncludePlexServer { get; }
    }

    public class GetPlexServerStatusByIdQueryValidator : AbstractValidator<GetPlexServerStatusByIdQuery>
    {
        public GetPlexServerStatusByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexServerStatusByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexServerStatusByIdQuery, Result<PlexServerStatus>>
    {
        public GetPlexServerStatusByIdQueryHandler(IPlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexServerStatus>> Handle(GetPlexServerStatusByIdQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.PlexServerStatuses.AsQueryable();

            if (request.IncludePlexServer)
            {
                query = query
                    .Include(x => x.PlexServer);
            }

            var status = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (status == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexServerStatus), request.Id);
            }
            return Result.Ok(status);
        }
    }
}