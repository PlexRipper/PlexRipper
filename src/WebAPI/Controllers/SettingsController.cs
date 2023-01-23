using Application.Contracts;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
using PlexRipper.Settings.Models;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SettingsController : BaseController
{
    private readonly IUserSettings _userSettings;

    public SettingsController(
        IMapper mapper,
        IUserSettings userSettings,
        INotificationsService notificationsService) : base(mapper, notificationsService)
    {
        _userSettings = userSettings;
    }

    // GET api/<SettingsController>/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<SettingsModelDTO>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public IActionResult GetSettings()
    {
        var settings = _userSettings.GetSettingsModel();
        return ToActionResult<ISettingsModel, SettingsModelDTO>(Result.Ok(settings));
    }

    // PUT api/<SettingsController>/
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<SettingsModelDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public IActionResult UpdateSettings([FromBody] SettingsModelDTO settingsModelDto)
    {
        var settings = _mapper.Map<SettingsModel>(settingsModelDto);
        var updateResult = _userSettings.UpdateSettings(settings);
        if (updateResult.IsFailed)
            return ToActionResult(updateResult.ToResult());

        return ToActionResult<ISettingsModel, SettingsModelDTO>(Result.Ok(_userSettings.GetSettingsModel()));
    }
}