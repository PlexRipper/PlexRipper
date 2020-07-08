using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        private readonly IPlexDownloadService _plexDownloadService;
        private readonly IMapper _mapper;

        public DownloadController(IPlexDownloadService plexDownloadService, IMapper mapper)
        {
            _plexDownloadService = plexDownloadService;
            _mapper = mapper;
        }
        // GET: api/<DownloadController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // Get api/<DownloadController>/movie/{plexMovieId:int}?plexAccountId={id}
        [HttpGet("movie/{plexMovieId:int}")]
        public async Task<IActionResult> Get(int plexMovieId, int plexAccountId)
        {
            var result = await _plexDownloadService.DownloadMovieAsync(plexAccountId, plexMovieId);
            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Value);
        }

        // POST api/<DownloadController>/movie/
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<DownloadController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<DownloadController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
