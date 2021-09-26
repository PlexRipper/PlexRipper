using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.FluentResult;
using PlexRipper.WebAPI.SignalR.Common;

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
            return ToActionResult<List<Notification>, List<NotificationDTO>>(await _notificationsService.GetNotifications());
        }

        // PUT api/<NotificationController>/5
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> HideNotification(int id)
        {
            if (id <= 0)
            {
                return BadRequestInvalidId();
            }

            return ToActionResult<List<Notification>, List<NotificationDTO>>(await _notificationsService.HideNotification(id));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationDTO notificationDto)
        {
            var notification = _mapper.Map<Notification>(notificationDto);

            return ToActionResult(await _notificationsService.CreateNotification(notification));
        }

        // POST api/<NotificationController>/clear
        [HttpPost("clear")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> ClearAllNotifications()
        {
            return ToActionResult(await _notificationsService.ClearAllNotifications());
        }
    }
}