using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexServers.Queries
{
    public class GetAllPlexServersByPlexAccountQuery : IRequest<Result<List<PlexServer>>>
    {
        public GetAllPlexServersByPlexAccountQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetAllPlexServersByPlexAccountQueryValidator : AbstractValidator<GetAllPlexServersByPlexAccountQuery>
    {
        public GetAllPlexServersByPlexAccountQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetAllPlexServersByPlexAccountQueryHandler : BaseHandler,
        IRequestHandler<GetAllPlexServersByPlexAccountQuery, Result<List<PlexServer>>>
    {
        private readonly IPlexRipperDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetAllPlexServersByPlexAccountQueryHandler(IPlexRipperDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Result<List<PlexServer>>> Handle(GetAllPlexServersByPlexAccountQuery request,
            CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<GetAllPlexServersByPlexAccountQuery, GetAllPlexServersByPlexAccountQueryValidator>(request);
            if (result.IsFailed) return result;

            var serverList = await _dbContext
                .PlexAccountServers
                .Include(x => x.PlexServer)
                .Where(x => x.PlexAccountId == request.Id)
                .ProjectTo<PlexServer>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return ReturnResult(serverList);

        }
    }
}
