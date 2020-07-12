using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Domain.Types.FileSystem;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DirectoryController : ControllerBase
    {
        private readonly IFileSystem _fileSystem;

        public DirectoryController(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        // GET: api/<DirectoryController>?path=
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileSystemResult))]
        public IActionResult Get(string path)
        {
            return Ok(_fileSystem.LookupContents(path, false, true));
        }

        // POST api/<DirectoryController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<DirectoryController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<DirectoryController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
