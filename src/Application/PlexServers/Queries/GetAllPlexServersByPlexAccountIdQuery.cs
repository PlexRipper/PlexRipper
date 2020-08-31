using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.PlexServers.Queries
{
    public class GetAllPlexServersByPlexAccountIdQuery : IRequest<Result<List<PlexServer>>>
    {
        public GetAllPlexServersByPlexAccountIdQuery(int plexAccountId)
        {
            PlexAccountId = plexAccountId;
        }

        public int PlexAccountId { get; }
    }

    public class GetAllPlexServersByPlexAccountIdQueryValidator : AbstractValidator<GetAllPlexServersByPlexAccountIdQuery>
    {
        public GetAllPlexServersByPlexAccountIdQueryValidator()
        {
            RuleFor(x => x.PlexAccountId).GreaterThan(0);
        }
    }


    public class GetAllPlexServersByPlexAccountIdQueryHandler : BaseHandler,
        IRequestHandler<GetAllPlexServersByPlexAccountIdQuery, Result<List<PlexServer>>>
    {
        private readonly IMapper _mapper;

        public GetAllPlexServersByPlexAccountIdQueryHandler(IPlexRipperDbContext dbContext, IMapper mapper): base(dbContext)
        {
            _mapper = mapper;
        }

        public async Task<Result<List<PlexServer>>> Handle(GetAllPlexServersByPlexAccountIdQuery request,
            CancellationToken cancellationToken)
        {
            var serverList = await _dbContext
                .PlexAccountServers
                .Include(x => x.PlexServer)
                .Where(x => x.PlexAccountId == request.PlexAccountId)
                .ProjectTo<PlexServer>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return ReturnResult(serverList);

        }
    }
}
