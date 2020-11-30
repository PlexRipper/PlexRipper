using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
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

        private readonly IMapper _mapper;

        public SettingsController(
            ISettingsService settingsService,
            IMapper mapper,
            INotificationsService notificationsService) : base(mapper, notificationsService)
        {
            _settingsService = settingsService;
            _mapper = mapper;
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
                if (result.IsFailed)
                {
                    return InternalServerError(result);
                }

                return Ok(Result.Ok(result.Value));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // PUT api/<SettingsController>/
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> UpdateSettings([FromBody] SettingsModel settingsModel)
        {
            try
            {
                var result = await _settingsService.UpdateSettings(settingsModel);
                if (result.IsFailed)
                {
                    return BadRequest(result);
                }

                return Ok(Result.Ok(true));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}