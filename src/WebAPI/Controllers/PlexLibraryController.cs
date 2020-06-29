using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlexLibraryController : ControllerBase
    {

        private readonly IPlexService _plexService;
        private readonly IPlexLibraryService _plexLibraryService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;


        public PlexLibraryController(IPlexService plexService, IPlexLibraryService plexLibraryService, IAccountService accountService, IMapper mapper)
        {

            _plexService = plexService;
            _plexLibraryService = plexLibraryService;
            _accountService = accountService;
            _mapper = mapper;
        }

        // GET: api/<PlexLibrary>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<PlexLibrary>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, bool refresh = false)
        {
            if (id <= 0)
            {
                return BadRequest($"The Id can not be 0 when updating a {nameof(PlexLibrary)}");
            }

            var data = await _plexLibraryService.GetPlexLibraryAsync(id, refresh);
            if (data != null)
            {
                var result = _mapper.Map<PlexLibraryDTO>(data);
                Log.Debug($"Found {data.GetMediaCount} in library {data.Title} of type {data.Type}");
                return Ok(result);

            }
            string message = $"Could not find a {nameof(PlexLibrary)} with Id: {id}";
            Log.Warning(message);
            return NotFound(message);
        }

        // POST api/<PlexLibrary>
        [HttpPost]
        public void Post([FromBody] string value)
        {
            //TODO Not yet implemented

        }

        // PUT api/<PlexLibrary>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            //TODO Not yet implemented
        }

        // DELETE api/<PlexLibrary>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            //TODO Not yet implemented

        }
    }
}
