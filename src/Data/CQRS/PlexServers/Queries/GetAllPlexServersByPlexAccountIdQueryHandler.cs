using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexServers;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexServers
{
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

        public GetAllPlexServersByPlexAccountIdQueryHandler(PlexRipperDbContext dbContext, IMapper mapper) : base(dbContext)
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