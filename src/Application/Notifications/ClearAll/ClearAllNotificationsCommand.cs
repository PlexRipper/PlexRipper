using Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Deletes/Clears all <see cref="Notification">Notifications</see>.
/// </summary>
/// <returns>Returns the number of <see cref="Notification">Notifications</see> that have been deleted.</returns>
public record ClearAllNotificationsCommand : IRequest<Result<int>> { }

public class ClearAllNotificationsHandler : IRequestHandler<ClearAllNotificationsCommand, Result<int>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public ClearAllNotificationsHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<int>> Handle(ClearAllNotificationsCommand command, CancellationToken cancellationToken)
    {
        // Empty the table
        var deletedNotificationsCount = await _dbContext.Notifications.ExecuteDeleteAsync(cancellationToken);
        return Result.Ok(deletedNotificationsCount);
    }
}