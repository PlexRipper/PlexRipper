using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Identity.Contracts;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application;

public class AppCredentialsDTO
{
    [SetsRequiredMembers]
    public AppCredentialsDTO(string userName, string password, bool isDefaultCredentials)
    {
        UserName = userName;
        Password = password;
        IsDefaultCredentials = isDefaultCredentials;
    }

    public required string UserName { get; init; }

    public required string Password { get; init; }

    public required bool IsDefaultCredentials { get; set; }
}

public class GetAppCredentials : BaseEndpointWithoutRequest<AppCredentialsDTO>
{
    private readonly Serilog.ILogger _log;
    private readonly UserManager<AppUser> _userManager;

    public override string EndpointPath => ApiRoutes.AuthenticatedController;

    public GetAppCredentials(ILogger log, UserManager<AppUser> userManager)
    {
        _log = log.ForContext<GetAppCredentials>();
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
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);
        // There is only 1 app user in the database
        var user = await _userManager.Users.FirstOrDefaultAsync(ct);
        if (user is null)
        {
            var result = Result.Fail("No app user found in the database").LogError();
            await SendFluentResult(result, ct);
            return;
        }

        var isDefaultCredentials =
            user.UserName == DefaultUserAppCredentials.DefaultUsername
            && await _userManager.CheckPasswordAsync(user, DefaultUserAppCredentials.DefaultPassword);

        // Don't send back the real password as this is hidden anyway when updating the password
        await SendFluentResult(
            Result.Ok(new AppCredentialsDTO(user.UserName!, StringExtensions.GeneratePassword(), isDefaultCredentials)),
            x => x,
            ct
        );
    }
}
