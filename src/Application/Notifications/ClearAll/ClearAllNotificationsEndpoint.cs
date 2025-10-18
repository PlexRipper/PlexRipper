using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Deletes/Clears all <see cref="Notification">Notifications</see>.
/// </summary>
/// <returns>Returns the number of <see cref="Notification">Notifications</see> that have been deleted.</returns>
public class ClearAllNotificationsEndpoint : BaseEndpointWithoutRequest<CountResponseDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.NotificationController + "/clear";

    public ClearAllNotificationsEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<ClearAllNotificationsEndpoint>();
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
        _log.Here().DebugApiCall(HttpContext);
        // Empty the table
        var deletedNotificationsCount = await _dbContext.Notifications.ExecuteDeleteAsync(ct);
        await SendFluentResult(Result.Ok(new CountResponseDTO(deletedNotificationsCount)), x => x, ct);
    }
}
