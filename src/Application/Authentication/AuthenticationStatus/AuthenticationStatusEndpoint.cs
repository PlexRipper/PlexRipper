using Microsoft.AspNetCore.Http;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public class AuthenticationStatusEndpoint : BaseEndpointWithoutRequest<UserClaimsDTO>
{
    public override string EndpointPath => ApiRoutes.AuthenticatedController + "/status";

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
        {
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<UserClaimsDTO>));
            x.Produces(StatusCodes.Status401Unauthorized, typeof(BaseResultDTO));
            x.Produces(StatusCodes.Status500InternalServerError);
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Check if the user is authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            var result = Result.Ok(
                new UserClaimsDTO
                {
                    IsLoggedIn = true,
                    UserName = User.Identity.Name ?? "UNKNOWN USERNAME",
                    Claims = User.Claims.Select(c => c.Type).ToList(),
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
