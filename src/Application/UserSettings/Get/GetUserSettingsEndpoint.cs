namespace Reaparr.Application;

public class GetUserSettingsEndpoint : EndpointWithoutRequest<SettingsModelDTO>
{
    private readonly IUserSettings _userSettings;

    public GetUserSettingsEndpoint(IUserSettings userSettings)
    {
        _userSettings = userSettings;
    }

    public override void Configure()
    {
        Get(ApiRoutes.SettingsController + "/");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<SettingsModelDTO>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.FluentResult(Result.Ok(_userSettings), x => x.ToDTO(), ct);
    }
}
