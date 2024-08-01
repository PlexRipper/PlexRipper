using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using PlexRipper.Settings;
using Settings.Contracts;

namespace PlexRipper.Application;

public class UpdateUserSettingsEndpointRequest
{
    [FromBody]
    public SettingsModelDTO SettingsModelDto { get; set; }
}

public class UpdateUserSettingsEndpointRequestValidator : Validator<UpdateUserSettingsEndpointRequest>
{
    public UpdateUserSettingsEndpointRequestValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.SettingsModelDto).NotNull();
        RuleFor(x => x.SettingsModelDto.ConfirmationSettings).NotNull();
        RuleFor(x => x.SettingsModelDto.DateTimeSettings).NotNull();
        RuleFor(x => x.SettingsModelDto.DebugSettings).NotNull();
        RuleFor(x => x.SettingsModelDto.DisplaySettings).NotNull();
        RuleFor(x => x.SettingsModelDto.DownloadManagerSettings).NotNull();
        RuleFor(x => x.SettingsModelDto.GeneralSettings).NotNull();
        RuleFor(x => x.SettingsModelDto.LanguageSettings).NotNull();
        RuleFor(x => x.SettingsModelDto.ServerSettings).NotNull();
    }
}

public class UpdateUserSettingsEndpoint : BaseEndpoint<UpdateUserSettingsEndpointRequest, SettingsModelDTO>
{
    private readonly IUserSettings _userSettings;

    public override string EndpointPath => ApiRoutes.SettingsController + "/";

    public UpdateUserSettingsEndpoint(IUserSettings userSettings)
    {
        _userSettings = userSettings;
    }

    public override void Configure()
    {
        Put(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<SettingsModelDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(UpdateUserSettingsEndpointRequest req, CancellationToken ct)
    {
        _userSettings.UpdateSettings(req.SettingsModelDto.ToModel());

        var settings = _userSettings.GetSettingsModel();

        await SendFluentResult(Result.Ok(settings), x => x.ToDTO(), ct);
    }
}
