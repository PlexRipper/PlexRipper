using Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public class TestAuthenticatedEndpoint : BaseEndpointWithoutRequest<UserClaimsDTO>
{
    public override string EndpointPath => ApiRoutes.AuthenticatedController + "/auth-test";

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
        {
            // x.AutoTagOverride("Authentication");
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<UserClaimsDTO>));
            x.Produces(StatusCodes.Status401Unauthorized, typeof(ResultDTO));
            x.Produces(StatusCodes.Status500InternalServerError);
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Check if the user is authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            var result = Result.Ok(
                new UserClaimsDTO()
                {
                    IsLoggedIn = true,
                    UserName = User.Identity.Name,
                    Claims = User.Claims.Select(c => new { c.Type, c.Value }),
                }
            );
            await SendFluentResult(result, x => x, ct);
        }
        else
        {
            await SendFluentResult(ResultExtensions.Create401UnauthorizedResult(), ct);
        }
    }
}
