using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : BaseController
    {
        private readonly IPlexRipperDatabaseService _plexRipperDatabaseService;

        private readonly IUserSettings _userSettings;

        public SettingsController(
            IPlexRipperDatabaseService plexRipperDatabaseService,
            IUserSettings userSettings,
            IMapper mapper,
            INotificationsService notificationsService) : base(mapper, notificationsService)
        {
            _plexRipperDatabaseService = plexRipperDatabaseService;
            _userSettings = userSettings;
        }

        // GET api/<SettingsController>/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<SettingsModelDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public IActionResult GetSettings()
        {
            return ToActionResult<ISettingsModel, SettingsModelDTO>(Result.Ok((ISettingsModel)_userSettings));
        }

        // PUT api/<SettingsController>/
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<SettingsModelDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public IActionResult UpdateSettings([FromBody] SettingsModelDTO settingsModelDto)
        {
            // var updateResult = _userSettings.UpdateSettings(settingsModelDto);
            // if (updateResult.IsFailed)
            //     return ToActionResult(updateResult.ToResult());
            //
            return Ok();

            // return ToActionResult<ISettingsModel, SettingsModelDTO>(Result.Ok((ISettingsModel)_userSettings));
        }

        // PUT api/<SettingsController>/
        [HttpPut("General")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<GeneralSettingsDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public IActionResult UpdateGeneralSettings([FromBody] GeneralSettingsDTO settingsModelDto)
        {
            // var updateResult = _userSettings.UpdateSettings(settingsModelDto);
            // if (updateResult.IsFailed)
            //     return ToActionResult(updateResult.ToResult());
            //
            // return ToActionResult<ISettingsModel, SettingsModelDTO>(Result.Ok((ISettingsModel)_userSettings));
            return Ok();
        }

        // GET api/<SettingsController>/ResetDb
        [HttpGet("ResetDb")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> ResetDatabase()
        {
            return ToActionResult(await _plexRipperDatabaseService.ResetDatabase());
        }
    }
}