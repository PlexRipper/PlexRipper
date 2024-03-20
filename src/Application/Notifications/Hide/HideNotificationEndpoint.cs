using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record HideNotificationEndpointRequest(int NotificationId);

public class HideNotificationEndpointRequestValidator : Validator<HideNotificationEndpointRequest>
{
    public HideNotificationEndpointRequestValidator()
    {
        RuleFor(x => x.NotificationId).GreaterThan(0);
    }
}

public class HideNotificationEndpoint : BaseCustomEndpoint<HideNotificationEndpointRequest, ResultDTO>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.NotificationController + "/{NotificationId}";

    public HideNotificationEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(HideNotificationEndpointRequest req, CancellationToken ct)
    {
        await _dbContext.Notifications
            .Where(x => x.Id == req.NotificationId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Hidden, true), ct);

        await SendFluentResult(Result.Ok(), ct);
    }
}