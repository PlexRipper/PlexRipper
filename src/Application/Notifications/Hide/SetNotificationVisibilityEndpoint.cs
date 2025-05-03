using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record SetNotificationVisibilityEndpointRequest
{
    public int Id { get; init; }

    public bool Hidden { get; init; } = true;
}

public class SetNotificationVisibilityEndpointRequestValidator : Validator<SetNotificationVisibilityEndpointRequest>
{
    public SetNotificationVisibilityEndpointRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Hidden).Equal(true);
    }
}

public class SetNotificationVisibilityEndpoint : BaseEndpoint<SetNotificationVisibilityEndpointRequest, BaseResultDTO>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.NotificationController;

    public SetNotificationVisibilityEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Patch(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(SetNotificationVisibilityEndpointRequest req, CancellationToken ct)
    {
        var changed = await _dbContext
            .Notifications.Where(x => x.Id == req.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Hidden, req.Hidden), ct);

        if (changed == 0)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(Notification), req.Id), ct);
            return;
        }

        await SendFluentResult(Result.Ok(), ct);
    }
}
