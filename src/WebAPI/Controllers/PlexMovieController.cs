using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlexMovieController : BaseController
    {
        // GET: api/<PlexMovieController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<PlexMovieController>/5
        [HttpGet("{id:int}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) { return BadRequestInvalidId; }

            return Ok();
        }

        // POST api/<PlexMovieController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<PlexMovieController>/5
        [HttpPut("{id:int}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PlexMovieController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
