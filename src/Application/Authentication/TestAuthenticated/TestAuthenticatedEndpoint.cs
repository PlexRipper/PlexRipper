using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PlexRipper.Identity.Contracts;

namespace PlexRipper.Application;

public class TestAuthenticatedEndpoint : EndpointWithoutRequest
{
    public string EndpointPath => ApiRoutes.TestAuthenticatedController;

    private readonly SignInManager<AppUser> _signInManager;

    public TestAuthenticatedEndpoint(SignInManager<AppUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
        {
            x.AutoTagOverride("Authentication");
            x.Produces(StatusCodes.Status200OK);
            x.Produces(StatusCodes.Status401Unauthorized);
            x.Produces(StatusCodes.Status500InternalServerError);
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Check if the user is authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            await SendOkAsync(
                new
                {
                    IsLoggedIn = true,
                    UserName = User.Identity.Name,
                    Claims = User.Claims.Select(c => new { c.Type, c.Value }),
                },
                ct
            );
        }
        else
        {
            await SendUnauthorizedAsync(ct);
        }
    }
}
