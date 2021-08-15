using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common;
using PlexRipper.Settings.Models;
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
        public IActionResult UpdateSettings([FromBody] SettingsModelDTO settingsModelDTO)
        {
            var settingsModel = _mapper.Map<SettingsModel>(settingsModelDTO);
            var updateResult = _userSettings.UpdateSettings(settingsModel);
            if (updateResult.IsFailed)
            {
                return ToActionResult(updateResult);
            }

            return ToActionResult<ISettingsModel, SettingsModelDTO>(Result.Ok((ISettingsModel)_userSettings));
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