using FastEndpoints.Security;
using Reaparr.Application.Contracts;
using Reaparr.Identity.Contracts;

namespace Reaparr.Application;

public class AppUserLogOutEndpoint : BaseEndpointWithoutRequest<string>
{
    public override string EndpointPath => ApiRoutes.LogOutEndpoint;

    private readonly ISignInService _signInService;

    public AppUserLogOutEndpoint(ISignInService signInService)
    {
        _signInService = signInService;
    }

    public override void Configure()
    {
        Post(EndpointPath);
        Description(x =>
        {
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<string>));
            x.Produces(StatusCodes.Status401Unauthorized);
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await _signInService.SignOutAsync();

        await CookieAuth.SignOutAsync();

        var result = Result.Ok("Logout successful");
        await SendFluentResult(result, x => x, ct);
    }
}
