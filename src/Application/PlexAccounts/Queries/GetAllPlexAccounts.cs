using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexAccounts
{
    public class GetAllPlexAccountsQuery : IRequest<ValidationResponse<List<PlexAccount>>>
    {

        public bool OnlyEnabled { get; }

        public GetAllPlexAccountsQuery(bool onlyEnabled = false)
        {
            OnlyEnabled = onlyEnabled;
        }
    }

    public class GetAllPlexAccountsQueryValidator : AbstractValidator<GetAllPlexAccountsQuery>
    {
        public GetAllPlexAccountsQueryValidator()
        {

        }
    }


    public class
        GetAllPlexAccountsHandler : IRequestHandler<GetAllPlexAccountsQuery, ValidationResponse<List<PlexAccount>>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetAllPlexAccountsHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ValidationResponse<List<PlexAccount>>> Handle(GetAllPlexAccountsQuery request,
            CancellationToken cancellationToken)
        {
            if (request.OnlyEnabled)
            {
                var plexAccounts = await _dbContext.PlexAccounts.Where(x => x.IsEnabled).ToListAsync();
                return new ValidationResponse<List<PlexAccount>>(plexAccounts);

            }
            else
            {
                var plexAccounts = await _dbContext.PlexAccounts.ToListAsync();
                return new ValidationResponse<List<PlexAccount>>(plexAccounts);
            }
        }
    }
}
