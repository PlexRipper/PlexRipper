using Application.Contracts;
using Data.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Deletes/Clears all <see cref="Notification">Notifications</see>.
/// </summary>
/// <returns>Returns the number of <see cref="Notification">Notifications</see> that have been deleted.</returns>
public class ClearAllNotificationsEndpoint : BaseCustomEndpointWithoutRequest<int>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.NotificationController + "/clear";

    public ClearAllNotificationsEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Delete(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<int>))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Empty the table
        var deletedNotificationsCount = await _dbContext.Notifications.ExecuteDeleteAsync(ct);
        await SendFluentResult(Result.Ok(deletedNotificationsCount), x => x, ct);
    }
}