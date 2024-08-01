using Application.Contracts;
using Microsoft.AspNetCore.Http;
using PlexRipper.Settings;
using Settings.Contracts;

namespace PlexRipper.Application;

public class GetUserSettingsEndpoint : BaseEndpointWithoutRequest<SettingsModelDTO>
{
    private readonly IUserSettings _userSettings;

    public override string EndpointPath => ApiRoutes.SettingsController + "/";

    public GetUserSettingsEndpoint(IUserSettings userSettings)
    {
        _userSettings = userSettings;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<SettingsModelDTO>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendFluentResult(Result.Ok(_userSettings), x => x.ToDTO(), ct);
    }
}
