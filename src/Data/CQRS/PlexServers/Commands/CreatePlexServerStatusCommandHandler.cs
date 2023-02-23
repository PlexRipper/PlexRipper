using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers;

public class CreatePlexServerStatusCommandValidator : AbstractValidator<CreatePlexServerStatusCommand>
{
    public CreatePlexServerStatusCommandValidator()
    {
        RuleFor(x => x.PlexServerStatus).NotNull();
        RuleFor(x => x.PlexServerStatus.Id).Equal(0);
        RuleFor(x => x.PlexServerStatus.PlexServerId).GreaterThan(0);
        RuleFor(x => x.PlexServerStatus.PlexServerConnectionId).GreaterThan(0);
        RuleFor(x => x.PlexServerStatus.LastChecked).NotNull();
        RuleFor(x => x.PlexServerStatus.StatusMessage).NotEmpty();
    }
}

public class CreatePlexServerStatusCommandHandler : BaseHandler, IRequestHandler<CreatePlexServerStatusCommand, Result<int>>
{
    public CreatePlexServerStatusCommandHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<int>> Handle(CreatePlexServerStatusCommand command, CancellationToken cancellationToken)
    {
        _log.Debug("Creating a new PlexServerStatus {@Status} in the database for {ServerId} ", command.PlexServerStatus,
            command.PlexServerStatus.PlexServerId);

        command.PlexServerStatus.PlexServer = null;
        command.PlexServerStatus.PlexServerConnection = null;
        await _dbContext.PlexServerStatuses.AddAsync(command.PlexServerStatus, cancellationToken);

        await SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(command.PlexServerStatus).GetDatabaseValuesAsync(cancellationToken);

        return Result.Ok(command.PlexServerStatus.Id);
    }
}