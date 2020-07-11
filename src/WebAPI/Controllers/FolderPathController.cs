using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderPathController : BaseController
    {
        private readonly IFolderPathService _folderPathService;

        public FolderPathController(IFolderPathService folderPathService)
        {
            _folderPathService = folderPathService;
        }

        // GET: api/<FolderPathController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _folderPathService.GetAllFolderPathsAsync();
            return Ok(result.Value);
        }

        // GET api/<FolderPathController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<FolderPathController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<FolderPathController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<FolderPathController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
