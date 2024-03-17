using Application.Contracts;
using Microsoft.AspNetCore.Http;
using PlexRipper.Settings;
using Settings.Contracts;

namespace PlexRipper.Application;

public class GetUserSettingsEndpoint : BaseCustomEndpointWithoutRequest
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
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<SettingsModelDTO>))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var settings = _userSettings.GetSettingsModel();
        await SendResult(Result.Ok(settings.ToDTO()), ct);
    }
}