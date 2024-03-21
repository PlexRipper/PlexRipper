using Application.Contracts;
using Data.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class GetAllNotificationsEndpoint : BaseEndpointWithoutRequest<List<NotificationDTO>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.NotificationController + "/";

    public GetAllNotificationsEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<NotificationDTO>>))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var list = await _dbContext.Notifications.ToListAsync(ct);
        await SendFluentResult(Result.Ok(list), x => x.ToDTO(), ct);
    }
}