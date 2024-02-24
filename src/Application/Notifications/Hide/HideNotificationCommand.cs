using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record HideNotificationCommand(int Id) : IRequest<Result>;

public class HideNotificationValidator : AbstractValidator<HideNotificationCommand>
{
    public HideNotificationValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class HideNotificationHandler : IRequestHandler<HideNotificationCommand, Result>
{
    private readonly IPlexRipperDbContext _dbContext;

    public HideNotificationHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(HideNotificationCommand command, CancellationToken cancellationToken)
    {
        await _dbContext.Notifications
            .Where(x => x.Id == command.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Hidden, true), cancellationToken);

        return Result.Ok();
    }
}