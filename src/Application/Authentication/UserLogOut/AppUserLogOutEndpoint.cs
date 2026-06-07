using FastEndpoints.Security;

namespace Reaparr.Application;

public class AppUserLogOutEndpoint : BaseEndpointWithoutRequest<string>
{
    public override string EndpointPath => ApiRoutes.LogOutEndpoint;

    private readonly IIdentitySignInService _identitySignInService;

    public AppUserLogOutEndpoint(IIdentitySignInService identitySignInService)
    {
        _identitySignInService = identitySignInService;
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
        await _identitySignInService.SignOutAsync();

        await CookieAuth.SignOutAsync();

        var result = Result.Ok("Logout successful");
        await Send.FluentResult(result, x => x, ct);
    }
}
