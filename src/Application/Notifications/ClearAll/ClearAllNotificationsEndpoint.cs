using Application.Contracts;
using Data.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Deletes/Clears all <see cref="Notification">Notifications</see>.
/// </summary>
/// <returns>Returns the number of <see cref="Notification">Notifications</see> that have been deleted.</returns>
public class ClearAllNotificationsEndpoint : BaseEndpointWithoutRequest<CountResponseDTO>
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

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<CountResponseDTO>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Empty the table
        var deletedNotificationsCount = await _dbContext.Notifications.ExecuteDeleteAsync(ct);
        await SendFluentResult(Result.Ok(new CountResponseDTO(deletedNotificationsCount)), x => x, ct);
    }
}
