using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers
{
    public class UpdatePlexServersCommandHandlerValidator : AbstractValidator<UpdatePlexServersCommand>
    {
        public UpdatePlexServersCommandHandlerValidator()
        {
            RuleForEach(x => x.PlexServers).ChildRules(plexServer => { plexServer.RuleFor(x => x.Id).GreaterThan(0); });
        }
    }

    public class UpdatePlexServersCommandHandlerHandler : BaseHandler, IRequestHandler<UpdatePlexServersCommand, Result>
    {
        public UpdatePlexServersCommandHandlerHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result> Handle(UpdatePlexServersCommand command, CancellationToken cancellationToken)
        {
            command.PlexServers.ForEach(x =>
            {
                x.PlexLibraries = null;
                x.ServerStatus = null;
                x.PlexAccountServers = null;
            });

            _dbContext.PlexServers.UpdateRange(command.PlexServers);
            await SaveChangesAsync();
            return Result.Ok();
        }
    }
}