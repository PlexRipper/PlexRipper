using System;
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
        private readonly ISettingsService _settingsService;

        public SettingsController(
            ISettingsService settingsService,
            IMapper mapper,
            INotificationsService notificationsService) : base(mapper, notificationsService)
        {
            _settingsService = settingsService;
        }

        // GET api/<SettingsController>/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<SettingsModel>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public IActionResult GetSettings()
        {
            try
            {
                var result = _settingsService.GetSettings();
                return result.IsFailed ? InternalServerError(result) : Ok(result);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // PUT api/<SettingsController>/
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<SettingsModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public IActionResult UpdateSettings([FromBody] SettingsModel settingsModel)
        {
            try
            {
                var result = _settingsService.UpdateSettings(settingsModel);
                return result.IsFailed ? BadRequest(result) : Ok(result);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}