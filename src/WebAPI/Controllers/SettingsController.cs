using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common;
using PlexRipper.Application.Settings.Models;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : BaseController
    {
        private readonly IPlexRipperDatabaseService _plexRipperDatabaseService;

        private readonly ISettingsService _settingsService;

        public SettingsController(
            IPlexRipperDatabaseService plexRipperDatabaseService,
            ISettingsService settingsService,
            IMapper mapper,
            INotificationsService notificationsService) : base(mapper, notificationsService)
        {
            _plexRipperDatabaseService = plexRipperDatabaseService;
            _settingsService = settingsService;
        }

        // GET api/<SettingsController>/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<SettingsModel>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public IActionResult GetSettings()
        {
            return ToActionResult(_settingsService.GetSettings());
        }

        // PUT api/<SettingsController>/
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<SettingsModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public IActionResult UpdateSettings([FromBody] SettingsModel settingsModel)
        {
            return ToActionResult(_settingsService.UpdateSettings(settingsModel));
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