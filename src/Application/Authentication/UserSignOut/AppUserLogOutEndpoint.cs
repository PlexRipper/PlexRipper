using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PlexRipper.Identity.Contracts;

namespace PlexRipper.Application;

public class AppUserLogOutEndpoint : BaseEndpointWithoutRequest
{
    public override string EndpointPath => ApiRoutes.LogOutController;

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
            x.AutoTagOverride("Authentication");
            x.Produces(StatusCodes.Status200OK);
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await _signInManager.SignOutAsync();

        await CookieAuth.SignOutAsync();

        await SendOkAsync(ct);
    }
}
