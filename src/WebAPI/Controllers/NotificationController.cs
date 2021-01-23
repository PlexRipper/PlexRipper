using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.SignalR.Common;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : BaseController
    {
        public NotificationController(INotificationsService notificationsService, IMapper mapper) : base(mapper, notificationsService) { }

        // GET api/<NotificationController>/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<NotificationDTO>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var result = await _notificationsService.GetNotifications();
                if (result.IsFailed)
                {
                    return InternalServerError(result);
                }

                var mapResult = _mapper.Map<List<NotificationDTO>>(result.Value);
                return Ok(Result.Ok(mapResult));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // PUT api/<NotificationController>/5
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> HideNotification(int id)
        {
            if (id <= 0)
            {
                return BadRequestInvalidId();
            }

            try
            {
                await _notificationsService.HideNotification(id);
                Log.Debug($"Setting the active plex account to {id}");
                return Ok(Result.Ok(true));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationDTO notificationDto)
        {
            var notification = _mapper.Map<Notification>(notificationDto);

            var result = await _notificationsService.CreateNotification(notification);
            if (result.IsFailed)
            {
                return InternalServerError(result);
            }

            return Ok(Result.Ok(true));
        }
    }
}