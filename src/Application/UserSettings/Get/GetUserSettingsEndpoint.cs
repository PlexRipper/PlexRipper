using Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Identity.Contracts;
using PlexRipper.Settings;
using Settings.Contracts;

namespace PlexRipper.Application;

public class GetUserSettingsEndpoint : BaseEndpointWithoutRequest<SettingsModelDTO>
{
    private readonly IUserSettings _userSettings;
    private readonly UserManager<AppUser> _userManager;

    public override string EndpointPath => ApiRoutes.SettingsController + "/";

    public GetUserSettingsEndpoint(IUserSettings userSettings, UserManager<AppUser> userManager)
    {
        _userSettings = userSettings;
        _userManager = userManager;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<SettingsModelDTO>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // There is only 1 app user in the database
        var user = await _userManager.Users.FirstOrDefaultAsync(ct);

        await SendFluentResult(Result.Ok(_userSettings), x => x.ToDTO(user!.UserName ?? ""), ct);
    }
}
