using FastEndpoints.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Reaparr.Application.Contracts;
using Reaparr.Identity.Contracts;

namespace Reaparr.Application;

public class AppUserLogOutEndpoint : BaseEndpointWithoutRequest<string>
{
    public override string EndpointPath => ApiRoutes.LogOutEndpoint;

    private readonly SignInManager<AppUser> _signInManager;

    public AppUserLogOutEndpoint(SignInManager<AppUser> signInManager)
    {
        _signInManager = signInManager;
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
        await _signInManager.SignOutAsync();

        await CookieAuth.SignOutAsync();

        var result = Result.Ok("Logout successful");
        await SendFluentResult(result, x => x, ct);
    }
}
