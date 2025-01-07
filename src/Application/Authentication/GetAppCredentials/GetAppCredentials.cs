using Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Identity.Contracts;

namespace PlexRipper.Application;

public class AppCredentialsDTO
{
    public AppCredentialsDTO(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }

    public string UserName { get; set; }

    public string Password { get; set; }
}

public class GetAppCredentials : BaseEndpointWithoutRequest<AppCredentialsDTO>
{
    private readonly UserManager<AppUser> _userManager;

    public override string EndpointPath => ApiRoutes.AuthenticatedController;

    public GetAppCredentials(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Summary(s =>
        {
            s.Summary = "Gets the username and random password from the main App user.";
        });

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<AppCredentialsDTO>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // There is only 1 app user in the database
        var user = await _userManager.Users.FirstOrDefaultAsync(ct);
        if (user is null)
        {
            var result = Result.Fail("No app user found in the database").LogError();
            await SendFluentResult(result, ct);
            return;
        }

        // Don't send back the real password as this is hidden anyway when updating the password
        await SendFluentResult(
            Result.Ok(new AppCredentialsDTO(user.UserName!, StringExtensions.GeneratePassword())),
            x => x,
            ct
        );
    }
}
