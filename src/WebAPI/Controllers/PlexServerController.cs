using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.WebAPI.Common.DTO;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlexServerController : ControllerBase
    {

        private readonly IPlexService _plexService;
        private readonly IPlexServerService _plexServerService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private ILogger Log { get; }

        public PlexServerController(IPlexService plexService, IPlexServerService plexServerService, IAccountService accountService, IMapper mapper, ILogger logger)
        {
            Log = logger.ForContext<AccountController>();
            _mapper = mapper;
            _plexService = plexService;
            _plexServerService = plexServerService;
            _accountService = accountService;
        }


        // GET: api/<PlexServerController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<PlexServerController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<PlexServerController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<PlexServerController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PlexServerController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        // GET api/<PlexServerController>/5
        [HttpGet("/ByAccount/{accountId}")]
        public async Task<IActionResult> GetByAccountId(int accountId)
        {
            string message;
            if (accountId <= 0)
            {
                message = $"The {nameof(PlexServer)}s can't be found if the {nameof(Account)} id is 0 or lower.";
                Log.Warning(message);
                return BadRequest(message);
            }

            var result = await _accountService.GetServersAsync(accountId);
            if (!result.Any())
            {
                message = $"Could not find any {nameof(PlexServer)}s associated with {nameof(Account)} where Id is {accountId}";
                Log.Debug(message);
                return NotFound(message);
            }
            return Ok(_mapper.Map<PlexServerDTO>(result));

        }
    }
}
