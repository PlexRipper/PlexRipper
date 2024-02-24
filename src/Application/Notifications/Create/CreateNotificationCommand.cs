using Data.Contracts;
using FluentValidation;

namespace PlexRipper.Application;

/// <summary>
/// Creates a <see cref="Notification"/> in the database.
/// </summary>
/// <param name="Notification">The Notification to create.</param>
/// <returns>The Id of the created <see cref="Notification"/>.</returns>
public record CreateNotificationCommand(Notification Notification) : IRequest<Result<int>>;

public class CreateNotificationValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationValidator()
    {
        RuleFor(x => x.Notification).NotNull();
        RuleFor(x => x.Notification.Message).NotEmpty();
        RuleFor(x => x.Notification.Level).NotEqual(NotificationLevel.None);
        RuleFor(x => x.Notification.CreatedAt).NotEqual(DateTime.MinValue);
    }
}

public class CreateNotificationHandler : IRequestHandler<CreateNotificationCommand, Result<int>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public CreateNotificationHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<int>> Handle(CreateNotificationCommand command, CancellationToken cancellationToken)
    {
        await _dbContext.Notifications.AddAsync(command.Notification, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok(command.Notification.Id);
    }
}