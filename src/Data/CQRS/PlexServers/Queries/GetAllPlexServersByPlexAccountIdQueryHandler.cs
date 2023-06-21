using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers;

public class GetAllPlexServersByPlexAccountIdQueryValidator : AbstractValidator<GetAllPlexServersByPlexAccountIdQuery>
{
    public GetAllPlexServersByPlexAccountIdQueryValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class GetAllPlexServersByPlexAccountIdQueryHandler : BaseHandler, IRequestHandler<GetAllPlexServersByPlexAccountIdQuery, Result<List<PlexServer>>>
{
    private readonly IMapper _mapper;

    public GetAllPlexServersByPlexAccountIdQueryHandler(ILog log, PlexRipperDbContext dbContext, IMapper mapper) : base(log, dbContext)
    {
        _mapper = mapper;
    }

    public async Task<Result<List<PlexServer>>> Handle(GetAllPlexServersByPlexAccountIdQuery request, CancellationToken cancellationToken)
    {
        var serverList = await _dbContext
            .PlexAccountServers
            .Include(x => x.PlexServer)
            .ThenInclude(x => x.ServerStatus)
            .Include(x => x.PlexServer)
            .ThenInclude(x => x.PlexServerConnections)
            .ThenInclude(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(1))
            .Where(x => x.PlexAccountId == request.PlexAccountId)
            .ProjectTo<PlexServer>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return ReturnResult(serverList);
    }
}