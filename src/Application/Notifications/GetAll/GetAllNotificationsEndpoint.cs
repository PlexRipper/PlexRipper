using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public class GetAllNotificationsEndpoint : BaseEndpointWithoutRequest<List<NotificationDTO>>
{
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.NotificationController + "/";

    public GetAllNotificationsEndpoint(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<NotificationDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var list = await _dbContext.Notifications.ToListAsync(ct);
        await SendFluentResult(Result.Ok(list), x => x.ToDTO(), ct);
    }
}
