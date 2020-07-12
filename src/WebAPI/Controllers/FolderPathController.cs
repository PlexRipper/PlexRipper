using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FolderPath>))]
        public async Task<IActionResult> Get()
        {
            var result = await _folderPathService.GetAllFolderPathsAsync();
            return Ok(result.Value);
        }

    }
}
