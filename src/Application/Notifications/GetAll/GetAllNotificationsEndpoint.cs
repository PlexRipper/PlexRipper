namespace Reaparr.Application;

public class GetAllNotificationsEndpoint : EndpointWithoutRequest<List<NotificationDTO>>
{
    private readonly IReaparrDbContext _dbContext;

    public GetAllNotificationsEndpoint(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(ApiRoutes.NotificationController + "/");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<NotificationDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var list = await _dbContext.Notifications.ToListAsync(ct);
        await Send.FluentResult(Result.Ok(list), x => x.ToDTO(), ct);
    }
}
